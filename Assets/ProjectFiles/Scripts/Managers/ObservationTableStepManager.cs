using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectFiles.Scripts.Managers
{
    public class ObservationTableStepManager : MonoBehaviour
    {
        [Header("Main Panel & Trigger Button")]
        [SerializeField] private GameObject observationTablePanel;
        [SerializeField] private Button observationTableButton;

        [Header("Feedback Sprites")]
        [SerializeField] private Sprite correctSprite;
        [SerializeField] private Sprite wrongSprite;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip wrongSound;

        [Header("Quadrat 1 Dropdowns & Feedback Images")]
        [SerializeField] private TMP_Dropdown quad1SpeciesA;
        [SerializeField] private Image quad1SpeciesA_FeedbackImage;
        [SerializeField] private TMP_Dropdown quad1SpeciesB;
        [SerializeField] private Image quad1SpeciesB_FeedbackImage;
        [SerializeField] private TMP_Dropdown quad1SpeciesC;
        [SerializeField] private Image quad1SpeciesC_FeedbackImage;

        [Header("Quadrat 2 TMPs")]
        [SerializeField] private List<GameObject> quad2Tmps;

        [Header("Quadrat 3 TMPs")]
        [SerializeField] private List<GameObject> quad3Tmps;

        [Header("Number of Individuals (S) Dropdowns & Feedback Images")]
        [SerializeField] private TMP_Dropdown numIndivSpeciesA;
        [SerializeField] private Image numIndivSpeciesA_FeedbackImage;
        [SerializeField] private TMP_Dropdown numIndivSpeciesB;
        [SerializeField] private Image numIndivSpeciesB_FeedbackImage;
        [SerializeField] private TMP_Dropdown numIndivSpeciesC;
        [SerializeField] private Image numIndivSpeciesC_FeedbackImage;

        [Header("Number of Quadrat Studied (Q) TMPs")]
        [SerializeField] private List<GameObject> numQuadStudiedTmps;

        [Header("Density (D) Dropdowns & Feedback Images")]
        [SerializeField] private TMP_Dropdown densitySpeciesA;
        [SerializeField] private Image densitySpeciesA_FeedbackImage;
        [SerializeField] private TMP_Dropdown densitySpeciesB;
        [SerializeField] private Image densitySpeciesB_FeedbackImage;
        [SerializeField] private TMP_Dropdown densitySpeciesC;
        [SerializeField] private Image densitySpeciesC_FeedbackImage;

        [Header("Correct Option Indices (0 = 1st Option, 1 = 2nd, 2 = 3rd)")]
        [SerializeField] private int quad1SpeciesA_CorrectIndex = 0;
        [SerializeField] private int quad1SpeciesB_CorrectIndex = 0;
        [SerializeField] private int quad1SpeciesC_CorrectIndex = 0;
        [Space]
        [SerializeField] private int numIndivSpeciesA_CorrectIndex = 0;
        [SerializeField] private int numIndivSpeciesB_CorrectIndex = 0;
        [SerializeField] private int numIndivSpeciesC_CorrectIndex = 0;
        [Space]
        [SerializeField] private int densitySpeciesA_CorrectIndex = 0;
        [SerializeField] private int densitySpeciesB_CorrectIndex = 0;
        [SerializeField] private int densitySpeciesC_CorrectIndex = 0;

        // Internal State Tracking
        private int activeIndex = -1;
        private List<TMP_Dropdown> allDropdowns = new();
        private HashSet<TMP_Dropdown> userSelectedDropdowns = new();
        private HashSet<TMP_Dropdown> correctlyAnsweredDropdowns = new();
        private Dictionary<TMP_Dropdown, int> correctAnswers = new();
        private Dictionary<TMP_Dropdown, Image> feedbackImages = new();

        // Reveal Flags
        private bool isQuad2Revealed = false;
        private bool isQuad3Revealed = false;
        private bool isNumQuadStudiedRevealed = false;

        private CanvasGroup panelCanvasGroup;
        private FieldInfo mValueField = typeof(TMP_Dropdown).GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            allDropdowns = new List<TMP_Dropdown>
            {
                quad1SpeciesA, quad1SpeciesB, quad1SpeciesC,
                numIndivSpeciesA, numIndivSpeciesB, numIndivSpeciesC,
                densitySpeciesA, densitySpeciesB, densitySpeciesC
            };

            if (observationTablePanel != null)
            {
                panelCanvasGroup = observationTablePanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = observationTablePanel.AddComponent<CanvasGroup>();
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        private void OnEnable()
        {
            PageNavigationController.OnPageChanged += HandlePageChanged;
        }

        private void OnDisable()
        {
            PageNavigationController.OnPageChanged -= HandlePageChanged;
        }

        private void Start()
        {
            MapCorrectAnswers();
            MapFeedbackImages();

            if (observationTableButton != null)
            {
                observationTableButton.onClick.AddListener(OnObservationTableButtonClicked);
            }

            foreach (var dropdown in allDropdowns)
            {
                if (dropdown != null)
                {
                    dropdown.onValueChanged.AddListener((val) => OnDropdownValueChanged(dropdown, val));
                }
            }

            ClearAllDropdownTexts();
            HandlePageChanged(PageNavigationController.CurrentIndex);
        }

        private void LateUpdate()
        {
            foreach (var dropdown in allDropdowns)
            {
                if (dropdown != null && dropdown.gameObject.activeInHierarchy)
                {
                    if (!userSelectedDropdowns.Contains(dropdown))
                    {
                        if (dropdown.captionText != null)
                        {
                            dropdown.captionText.text = string.Empty;
                        }
                    }
                }
            }
        }

        private void MapCorrectAnswers()
        {
            correctAnswers.Clear();
            if (quad1SpeciesA != null) correctAnswers[quad1SpeciesA] = quad1SpeciesA_CorrectIndex;
            if (quad1SpeciesB != null) correctAnswers[quad1SpeciesB] = quad1SpeciesB_CorrectIndex;
            if (quad1SpeciesC != null) correctAnswers[quad1SpeciesC] = quad1SpeciesC_CorrectIndex;

            if (numIndivSpeciesA != null) correctAnswers[numIndivSpeciesA] = numIndivSpeciesA_CorrectIndex;
            if (numIndivSpeciesB != null) correctAnswers[numIndivSpeciesB] = numIndivSpeciesB_CorrectIndex;
            if (numIndivSpeciesC != null) correctAnswers[numIndivSpeciesC] = numIndivSpeciesC_CorrectIndex;

            if (densitySpeciesA != null) correctAnswers[densitySpeciesA] = densitySpeciesA_CorrectIndex;
            if (densitySpeciesB != null) correctAnswers[densitySpeciesB] = densitySpeciesB_CorrectIndex;
            if (densitySpeciesC != null) correctAnswers[densitySpeciesC] = densitySpeciesC_CorrectIndex;
        }

        private void MapFeedbackImages()
        {
            feedbackImages.Clear();
            if (quad1SpeciesA != null) feedbackImages[quad1SpeciesA] = quad1SpeciesA_FeedbackImage;
            if (quad1SpeciesB != null) feedbackImages[quad1SpeciesB] = quad1SpeciesB_FeedbackImage;
            if (quad1SpeciesC != null) feedbackImages[quad1SpeciesC] = quad1SpeciesC_FeedbackImage;

            if (numIndivSpeciesA != null) feedbackImages[numIndivSpeciesA] = numIndivSpeciesA_FeedbackImage;
            if (numIndivSpeciesB != null) feedbackImages[numIndivSpeciesB] = numIndivSpeciesB_FeedbackImage;
            if (numIndivSpeciesC != null) feedbackImages[numIndivSpeciesC] = numIndivSpeciesC_FeedbackImage;

            if (densitySpeciesA != null) feedbackImages[densitySpeciesA] = densitySpeciesA_FeedbackImage;
            if (densitySpeciesB != null) feedbackImages[densitySpeciesB] = densitySpeciesB_FeedbackImage;
            if (densitySpeciesC != null) feedbackImages[densitySpeciesC] = densitySpeciesC_FeedbackImage;

            HideAllFeedbackImages();
        }

        private void HideAllFeedbackImages()
        {
            foreach (var img in feedbackImages.Values)
            {
                if (img != null)
                {
                    img.gameObject.SetActive(false);
                }
            }
        }

        public void ClearAllDropdownTexts()
        {
            userSelectedDropdowns.Clear();
            correctlyAnsweredDropdowns.Clear();

            foreach (var dropdown in allDropdowns)
            {
                ResetDropdownState(dropdown);
            }
        }

        private void ResetDropdownState(TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;

            if (mValueField != null)
            {
                mValueField.SetValue(dropdown, -1);
            }

            if (dropdown.captionText != null)
            {
                dropdown.captionText.text = string.Empty;
            }

            if (feedbackImages.TryGetValue(dropdown, out Image feedbackImg) && feedbackImg != null)
            {
                feedbackImg.gameObject.SetActive(false);
            }
        }

        private void HandlePageChanged(int index)
        {
            Debug.Log($"[Navigation Debug] Page changed to Index: {index}");

            activeIndex = index;

            SetPanelActiveState(false);
            ResetAllControls();

            bool isObservationPage = (index >= 13 && index <= 25) && (index != 18 && index != 22);

            if (observationTableButton != null)
            {
                observationTableButton.gameObject.SetActive(isObservationPage);
                observationTableButton.interactable = isObservationPage;
            }
        }

        public void OnObservationTableButtonClicked()
        {
            Debug.Log($"[Observation Table] Button clicked at Page Index: {activeIndex}");

            SetPanelActiveState(true);
            ResetAllControls();

            switch (activeIndex)
            {
                case 13: EnableDropdown(quad1SpeciesA); break;
                case 14: EnableDropdown(quad1SpeciesB); break;
                case 15: EnableDropdown(quad1SpeciesC); break;

                case 16: 
                    isQuad2Revealed = true; 
                    SetTMPsState(quad2Tmps, true); 
                    UnlockPageNavigation(); 
                    break;

                case 17: 
                    isQuad3Revealed = true; 
                    SetTMPsState(quad3Tmps, true); 
                    UnlockPageNavigation(); 
                    break;

                case 18: break;

                case 19: EnableDropdown(numIndivSpeciesA); break;
                case 20: EnableDropdown(numIndivSpeciesB); break;
                case 21: EnableDropdown(numIndivSpeciesC); break;

                case 22: break;

                case 23: 
                    isNumQuadStudiedRevealed = true; 
                    SetTMPsState(numQuadStudiedTmps, true); 
                    EnableDropdown(densitySpeciesA); 
                    break;

                case 24: EnableDropdown(densitySpeciesB); break;
                case 25: EnableDropdown(densitySpeciesC); break;
            }
        }

        private void OnDropdownValueChanged(TMP_Dropdown dropdown, int selectedIndex)
        {
            if (selectedIndex >= 0 && dropdown != null)
            {
                userSelectedDropdowns.Add(dropdown);

                if (dropdown.captionText != null && dropdown.options.Count > selectedIndex)
                {
                    dropdown.captionText.text = dropdown.options[selectedIndex].text;
                }

                if (correctAnswers.TryGetValue(dropdown, out int expectedCorrectIndex))
                {
                    bool isCorrect = (selectedIndex == expectedCorrectIndex);

                    // Show correct or wrong feedback sprite
                    if (feedbackImages.TryGetValue(dropdown, out Image feedbackImg) && feedbackImg != null)
                    {
                        feedbackImg.sprite = isCorrect ? correctSprite : wrongSprite;
                        feedbackImg.gameObject.SetActive(true);
                    }

                    // Play sound
                    PlayFeedbackSound(isCorrect);

                    if (isCorrect)
                    {
                        Debug.Log($"[Observation Table] Correct selection on dropdown '{dropdown.name}' (Index: {selectedIndex}).");
                        correctlyAnsweredDropdowns.Add(dropdown);
                        dropdown.interactable = false;
                        UnlockPageNavigation();
                    }
                    else
                    {
                        Debug.LogWarning($"[Observation Table] Incorrect selection on '{dropdown.name}'. Selected: {selectedIndex}, Expected: {expectedCorrectIndex}");
                    }
                }
            }
        }

        private void PlayFeedbackSound(bool isCorrect)
        {
            if (audioSource == null) return;

            AudioClip clipToPlay = isCorrect ? correctSound : wrongSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        private void UnlockPageNavigation()
        {
            Debug.Log($"[Navigation Debug] Requesting Navigation Unlock for Page Index: {activeIndex}");
            PageNavigationController.RequestNavigationUnlock();
        }

        private void SetPanelActiveState(bool isActive)
        {
            if (observationTablePanel != null)
            {
                observationTablePanel.SetActive(isActive);
            }

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.blocksRaycasts = isActive;
            }
        }

        private void ResetAllControls()
        {
            foreach (var dropdown in allDropdowns)
            {
                DisableDropdown(dropdown);
            }

            // Turns off all feedback images when changing pages/slides
            HideAllFeedbackImages();

            SetTMPsState(quad2Tmps, isQuad2Revealed);
            SetTMPsState(quad3Tmps, isQuad3Revealed);
            SetTMPsState(numQuadStudiedTmps, isNumQuadStudiedRevealed);
        }

        private void EnableDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;

            dropdown.gameObject.SetActive(true);

            if (correctlyAnsweredDropdowns.Contains(dropdown))
            {
                dropdown.interactable = false;
                UnlockPageNavigation();
            }
            else
            {
                dropdown.interactable = true;

                if (!userSelectedDropdowns.Contains(dropdown))
                {
                    ResetDropdownState(dropdown);
                }
            }
        }

        private void DisableDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                dropdown.interactable = false;
            }
        }

        private void SetTMPsState(List<GameObject> tmps, bool state)
        {
            if (tmps == null) return;
            foreach (var tmp in tmps)
            {
                if (tmp != null)
                    tmp.SetActive(state);
            }
        }
    }
}