using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectFiles.Scripts.Managers
{
    public class ObservationTableStepManager : MonoBehaviour
    {
        [System.Serializable]
        public class TableData
        {
            [Header("Table Config")]
            public string tableName = "Observation Table";
            public GameObject observationTablePanel;

            [Header("Page Index Configuration")]
            public int quad1SpeciesA_PageIndex = -1;
            public int quad1SpeciesB_PageIndex = -1;
            public int quad1SpeciesC_PageIndex = -1;
            public int quad2Reveal_PageIndex = -1;
            public int quad3Reveal_PageIndex = -1;
            public int numIndivSpeciesA_PageIndex = -1;
            public int numIndivSpeciesB_PageIndex = -1;
            public int numIndivSpeciesC_PageIndex = -1;
            public int numQuadStudiedAndDensityA_PageIndex = -1;
            public int densitySpeciesB_PageIndex = -1;
            public int densitySpeciesC_PageIndex = -1;

            [Header("Quadrat 1 Dropdowns & Feedback Images")]
            public TMP_Dropdown quad1SpeciesA;
            public Image quad1SpeciesA_FeedbackImage;
            public TMP_Dropdown quad1SpeciesB;
            public Image quad1SpeciesB_FeedbackImage;
            public TMP_Dropdown quad1SpeciesC;
            public Image quad1SpeciesC_FeedbackImage;

            [Header("Quadrat 2 & 3 TMPs")]
            public List<GameObject> quad2Tmps;
            public List<GameObject> quad3Tmps;

            [Header("Number of Individuals (S) Dropdowns & Feedback Images")]
            public TMP_Dropdown numIndivSpeciesA;
            public Image numIndivSpeciesA_FeedbackImage;
            public TMP_Dropdown numIndivSpeciesB;
            public Image numIndivSpeciesB_FeedbackImage;
            public TMP_Dropdown numIndivSpeciesC;
            public Image numIndivSpeciesC_FeedbackImage;

            [Header("Number of Quadrat Studied (Q) TMPs")]
            public List<GameObject> numQuadStudiedTmps;

            [Header("Density (D) Dropdowns & Feedback Images")]
            public TMP_Dropdown densitySpeciesA;
            public Image densitySpeciesA_FeedbackImage;
            public TMP_Dropdown densitySpeciesB;
            public Image densitySpeciesB_FeedbackImage;
            public TMP_Dropdown densitySpeciesC;
            public Image densitySpeciesC_FeedbackImage;

            [Header("Correct Indices (0 = 1st Option, 1 = 2nd, 2 = 3rd)")]
            public int quad1SpeciesA_CorrectIndex = 0;
            public int quad1SpeciesB_CorrectIndex = 0;
            public int quad1SpeciesC_CorrectIndex = 0;
            [Space]
            public int numIndivSpeciesA_CorrectIndex = 0;
            public int numIndivSpeciesB_CorrectIndex = 0;
            public int numIndivSpeciesC_CorrectIndex = 0;
            [Space]
            public int densitySpeciesA_CorrectIndex = 0;
            public int densitySpeciesB_CorrectIndex = 0;
            public int densitySpeciesC_CorrectIndex = 0;

            [Header("Wrong Answer Feedback Messages")]
            [TextArea(2, 3)] public string quad1SpeciesA_WrongMsg = "Incorrect selection for Quadrat 1 - Species A. Please check your observation.";
            [TextArea(2, 3)] public string quad1SpeciesB_WrongMsg = "Incorrect selection for Quadrat 1 - Species B. Please check your observation.";
            [TextArea(2, 3)] public string quad1SpeciesC_WrongMsg = "Incorrect selection for Quadrat 1 - Species C. Please check your observation.";
            [Space]
            [TextArea(2, 3)] public string numIndivSpeciesA_WrongMsg = "Incorrect number of individuals for Species A.";
            [TextArea(2, 3)] public string numIndivSpeciesB_WrongMsg = "Incorrect number of individuals for Species B.";
            [TextArea(2, 3)] public string numIndivSpeciesC_WrongMsg = "Incorrect number of individuals for Species C.";
            [Space]
            [TextArea(2, 3)] public string densitySpeciesA_WrongMsg = "Incorrect density value calculated for Species A.";
            [TextArea(2, 3)] public string densitySpeciesB_WrongMsg = "Incorrect density value calculated for Species B.";
            [TextArea(2, 3)] public string densitySpeciesC_WrongMsg = "Incorrect density value calculated for Species C.";

            [Header("Reveal Page Messages")]
            [TextArea(2, 4)] public string quad2Reveal_Msg = "Quadrat 2 observations have been revealed.";
            [TextArea(2, 4)] public string quad3Reveal_Msg = "Quadrat 3 observations have been revealed.";
            [TextArea(2, 4)] public string numQuadStudied_Msg = "Total number of quadrats studied has been revealed.";

            // Runtime state tracked per table
            [HideInInspector] public bool isQuad2Revealed;
            [HideInInspector] public bool isQuad3Revealed;
            [HideInInspector] public bool isNumQuadStudiedRevealed;
            [HideInInspector] public CanvasGroup panelCanvasGroup;

            [HideInInspector] public List<TMP_Dropdown> allDropdowns = new();
            [HideInInspector] public HashSet<TMP_Dropdown> userSelectedDropdowns = new();
            [HideInInspector] public HashSet<TMP_Dropdown> correctlyAnsweredDropdowns = new();
            [HideInInspector] public Dictionary<TMP_Dropdown, int> correctAnswers = new();
            [HideInInspector] public Dictionary<TMP_Dropdown, Image> feedbackImages = new();
            [HideInInspector] public Dictionary<TMP_Dropdown, string> wrongMessages = new();
        }

        [Header("Global UI & Audio Settings")]
        [SerializeField] private Button observationTableButton;
        [SerializeField] private Sprite correctSprite;
        [SerializeField] private Sprite wrongSprite;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip wrongSound;

        [Header("Message Panel Settings")]
        [Tooltip("The UI Panel/Image that contains the text child component")]
        [SerializeField] private GameObject messagePanel;
        [Tooltip("The TextMeshPro text component child inside the message panel")]
        [SerializeField] private TMP_Text messageText;

        [Header("Highlight Colors")]
        [SerializeField] private Color activeHighlightColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;

        [Header("Observation Tables Setup")]
        [SerializeField] private List<TableData> observationTables = new();

        // Internal State Tracking
        private int activeIndex = -1;
        private TableData currentActiveTable = null;
        private FieldInfo mValueField = typeof(TMP_Dropdown).GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            foreach (var table in observationTables)
            {
                InitializeTable(table);
            }

            HideMessage();
        }

        private void InitializeTable(TableData table)
        {
            table.allDropdowns = new List<TMP_Dropdown>
            {
                table.quad1SpeciesA, table.quad1SpeciesB, table.quad1SpeciesC,
                table.numIndivSpeciesA, table.numIndivSpeciesB, table.numIndivSpeciesC,
                table.densitySpeciesA, table.densitySpeciesB, table.densitySpeciesC
            };

            if (table.observationTablePanel != null)
            {
                table.panelCanvasGroup = table.observationTablePanel.GetComponent<CanvasGroup>();
                if (table.panelCanvasGroup == null)
                {
                    table.panelCanvasGroup = table.observationTablePanel.AddComponent<CanvasGroup>();
                }
            }

            MapCorrectAnswers(table);
            MapFeedbackImages(table);
            MapWrongMessages(table);
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
            if (observationTableButton != null)
            {
                observationTableButton.onClick.AddListener(OnObservationTableButtonClicked);
            }

            foreach (var table in observationTables)
            {
                foreach (var dropdown in table.allDropdowns)
                {
                    if (dropdown != null)
                    {
                        dropdown.onValueChanged.AddListener((val) => OnDropdownValueChanged(table, dropdown, val));
                    }
                }
                ClearTableDropdownTexts(table);
            }

            HandlePageChanged(PageNavigationController.CurrentIndex);
        }

        private void LateUpdate()
        {
            if (currentActiveTable == null) return;

            foreach (var dropdown in currentActiveTable.allDropdowns)
            {
                if (dropdown != null && dropdown.gameObject.activeInHierarchy)
                {
                    if (!currentActiveTable.userSelectedDropdowns.Contains(dropdown))
                    {
                        if (dropdown.captionText != null)
                        {
                            dropdown.captionText.text = string.Empty;
                        }
                    }
                }
            }
        }

        private void MapCorrectAnswers(TableData table)
        {
            table.correctAnswers.Clear();
            if (table.quad1SpeciesA != null) table.correctAnswers[table.quad1SpeciesA] = table.quad1SpeciesA_CorrectIndex;
            if (table.quad1SpeciesB != null) table.correctAnswers[table.quad1SpeciesB] = table.quad1SpeciesB_CorrectIndex;
            if (table.quad1SpeciesC != null) table.correctAnswers[table.quad1SpeciesC] = table.quad1SpeciesC_CorrectIndex;

            if (table.numIndivSpeciesA != null) table.correctAnswers[table.numIndivSpeciesA] = table.numIndivSpeciesA_CorrectIndex;
            if (table.numIndivSpeciesB != null) table.correctAnswers[table.numIndivSpeciesB] = table.numIndivSpeciesB_CorrectIndex;
            if (table.numIndivSpeciesC != null) table.correctAnswers[table.numIndivSpeciesC] = table.numIndivSpeciesC_CorrectIndex;

            if (table.densitySpeciesA != null) table.correctAnswers[table.densitySpeciesA] = table.densitySpeciesA_CorrectIndex;
            if (table.densitySpeciesB != null) table.correctAnswers[table.densitySpeciesB] = table.densitySpeciesB_CorrectIndex;
            if (table.densitySpeciesC != null) table.correctAnswers[table.densitySpeciesC] = table.densitySpeciesC_CorrectIndex;
        }

        private void MapFeedbackImages(TableData table)
        {
            table.feedbackImages.Clear();
            if (table.quad1SpeciesA != null) table.feedbackImages[table.quad1SpeciesA] = table.quad1SpeciesA_FeedbackImage;
            if (table.quad1SpeciesB != null) table.feedbackImages[table.quad1SpeciesB] = table.quad1SpeciesB_FeedbackImage;
            if (table.quad1SpeciesC != null) table.feedbackImages[table.quad1SpeciesC] = table.quad1SpeciesC_FeedbackImage;

            if (table.numIndivSpeciesA != null) table.feedbackImages[table.numIndivSpeciesA] = table.numIndivSpeciesA_FeedbackImage;
            if (table.numIndivSpeciesB != null) table.feedbackImages[table.numIndivSpeciesB] = table.numIndivSpeciesB_FeedbackImage;
            if (table.numIndivSpeciesC != null) table.feedbackImages[table.numIndivSpeciesC] = table.numIndivSpeciesC_FeedbackImage;

            if (table.densitySpeciesA != null) table.feedbackImages[table.densitySpeciesA] = table.densitySpeciesA_FeedbackImage;
            if (table.densitySpeciesB != null) table.feedbackImages[table.densitySpeciesB] = table.densitySpeciesB_FeedbackImage;
            if (table.densitySpeciesC != null) table.feedbackImages[table.densitySpeciesC] = table.densitySpeciesC_FeedbackImage;

            HideAllFeedbackImages(table);
        }

        private void MapWrongMessages(TableData table)
        {
            table.wrongMessages.Clear();
            if (table.quad1SpeciesA != null) table.wrongMessages[table.quad1SpeciesA] = table.quad1SpeciesA_WrongMsg;
            if (table.quad1SpeciesB != null) table.wrongMessages[table.quad1SpeciesB] = table.quad1SpeciesB_WrongMsg;
            if (table.quad1SpeciesC != null) table.wrongMessages[table.quad1SpeciesC] = table.quad1SpeciesC_WrongMsg;

            if (table.numIndivSpeciesA != null) table.wrongMessages[table.numIndivSpeciesA] = table.numIndivSpeciesA_WrongMsg;
            if (table.numIndivSpeciesB != null) table.wrongMessages[table.numIndivSpeciesB] = table.numIndivSpeciesB_WrongMsg;
            if (table.numIndivSpeciesC != null) table.wrongMessages[table.numIndivSpeciesC] = table.numIndivSpeciesC_WrongMsg;

            if (table.densitySpeciesA != null) table.wrongMessages[table.densitySpeciesA] = table.densitySpeciesA_WrongMsg;
            if (table.densitySpeciesB != null) table.wrongMessages[table.densitySpeciesB] = table.densitySpeciesB_WrongMsg;
            if (table.densitySpeciesC != null) table.wrongMessages[table.densitySpeciesC] = table.densitySpeciesC_WrongMsg;
        }

        private void HideAllFeedbackImages(TableData table)
        {
            foreach (var img in table.feedbackImages.Values)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }

        private void ShowMessage(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            if (messageText != null)
            {
                messageText.text = content;
            }

            if (messagePanel != null)
            {
                messagePanel.SetActive(true);
            }
        }

        private void HideMessage()
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }

            if (messageText != null)
            {
                messageText.text = string.Empty;
            }
        }

        public void ClearTableDropdownTexts(TableData table)
        {
            table.userSelectedDropdowns.Clear();
            table.correctlyAnsweredDropdowns.Clear();

            foreach (var dropdown in table.allDropdowns)
            {
                ResetDropdownState(table, dropdown);
            }
        }

        private void ResetDropdownState(TableData table, TMP_Dropdown dropdown)
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

            if (table.feedbackImages.TryGetValue(dropdown, out Image feedbackImg) && feedbackImg != null)
            {
                feedbackImg.gameObject.SetActive(false);
            }
        }

        private bool IsTablePage(TableData table, int pageIndex)
        {
            if (pageIndex < 0) return false;

            return (table.quad1SpeciesA_PageIndex >= 0 && pageIndex == table.quad1SpeciesA_PageIndex) ||
                   (table.quad1SpeciesB_PageIndex >= 0 && pageIndex == table.quad1SpeciesB_PageIndex) ||
                   (table.quad1SpeciesC_PageIndex >= 0 && pageIndex == table.quad1SpeciesC_PageIndex) ||
                   (table.quad2Reveal_PageIndex >= 0 && pageIndex == table.quad2Reveal_PageIndex) ||
                   (table.quad3Reveal_PageIndex >= 0 && pageIndex == table.quad3Reveal_PageIndex) ||
                   (table.numIndivSpeciesA_PageIndex >= 0 && pageIndex == table.numIndivSpeciesA_PageIndex) ||
                   (table.numIndivSpeciesB_PageIndex >= 0 && pageIndex == table.numIndivSpeciesB_PageIndex) ||
                   (table.numIndivSpeciesC_PageIndex >= 0 && pageIndex == table.numIndivSpeciesC_PageIndex) ||
                   (table.numQuadStudiedAndDensityA_PageIndex >= 0 && pageIndex == table.numQuadStudiedAndDensityA_PageIndex) ||
                   (table.densitySpeciesB_PageIndex >= 0 && pageIndex == table.densitySpeciesB_PageIndex) ||
                   (table.densitySpeciesC_PageIndex >= 0 && pageIndex == table.densitySpeciesC_PageIndex);
        }

        private void HandlePageChanged(int index)
        {
            activeIndex = index;
            HideMessage();

            currentActiveTable = observationTables.Find(t => IsTablePage(t, index));

            foreach (var table in observationTables)
            {
                SetPanelActiveState(table, false);
                ResetAllControls(table);
            }

            bool isObservationPage = (currentActiveTable != null);

            if (observationTableButton != null)
            {
                observationTableButton.transform.SetAsLastSibling();
                observationTableButton.gameObject.SetActive(isObservationPage);
            }

            if (currentActiveTable != null && IsCurrentPageAlreadyCompleted(currentActiveTable, index))
            {
                OnObservationTableButtonClicked();
            }

            Debug.Log($"[Navigation Debug] Page changed to Index: {index} | Active Table Found: {(currentActiveTable != null ? currentActiveTable.tableName : "None")}");
        }

        private bool IsCurrentPageAlreadyCompleted(TableData table, int pageIndex)
        {
            if (pageIndex == table.quad1SpeciesA_PageIndex)
                return IsDropdownCompleted(table, table.quad1SpeciesA);

            if (pageIndex == table.quad1SpeciesB_PageIndex)
                return IsDropdownCompleted(table, table.quad1SpeciesB);

            if (pageIndex == table.quad1SpeciesC_PageIndex)
                return IsDropdownCompleted(table, table.quad1SpeciesC);

            if (pageIndex == table.quad2Reveal_PageIndex)
                return table.isQuad2Revealed;

            if (pageIndex == table.quad3Reveal_PageIndex)
                return table.isQuad3Revealed;

            if (pageIndex == table.numIndivSpeciesA_PageIndex)
                return IsDropdownCompleted(table, table.numIndivSpeciesA);

            if (pageIndex == table.numIndivSpeciesB_PageIndex)
                return IsDropdownCompleted(table, table.numIndivSpeciesB);

            if (pageIndex == table.numIndivSpeciesC_PageIndex)
                return IsDropdownCompleted(table, table.numIndivSpeciesC);

            if (pageIndex == table.numQuadStudiedAndDensityA_PageIndex)
                return table.isNumQuadStudiedRevealed && IsDropdownCompleted(table, table.densitySpeciesA);

            if (pageIndex == table.densitySpeciesB_PageIndex)
                return IsDropdownCompleted(table, table.densitySpeciesB);

            if (pageIndex == table.densitySpeciesC_PageIndex)
                return IsDropdownCompleted(table, table.densitySpeciesC);

            return false;
        }

        private bool IsDropdownCompleted(TableData table, TMP_Dropdown dropdown)
        {
            return dropdown != null && table.correctlyAnsweredDropdowns.Contains(dropdown);
        }

        public void OnObservationTableButtonClicked()
        {
            if (currentActiveTable == null) return;

            Debug.Log($"[Observation Table] Button clicked for {currentActiveTable.tableName} at Page Index: {activeIndex}");

            foreach (var table in observationTables)
            {
                if (table != currentActiveTable)
                {
                    SetPanelActiveState(table, false);
                }
            }

            SetPanelActiveState(currentActiveTable, true);
            ResetAllControls(currentActiveTable);

            // Hide previous messages on opening table panel
            HideMessage();

            if (activeIndex == currentActiveTable.quad1SpeciesA_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.quad1SpeciesA);
            }
            else if (activeIndex == currentActiveTable.quad1SpeciesB_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.quad1SpeciesB);
            }
            else if (activeIndex == currentActiveTable.quad1SpeciesC_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.quad1SpeciesC);
            }
            else if (activeIndex == currentActiveTable.quad2Reveal_PageIndex)
            {
                currentActiveTable.isQuad2Revealed = true;
                SetTMPsState(currentActiveTable.quad2Tmps, true, true);
                ShowMessage(currentActiveTable.quad2Reveal_Msg);
                UnlockPageNavigation();
            }
            else if (activeIndex == currentActiveTable.quad3Reveal_PageIndex)
            {
                currentActiveTable.isQuad3Revealed = true;
                SetTMPsState(currentActiveTable.quad3Tmps, true, true);
                ShowMessage(currentActiveTable.quad3Reveal_Msg);
                UnlockPageNavigation();
            }
            else if (activeIndex == currentActiveTable.numIndivSpeciesA_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.numIndivSpeciesA);
            }
            else if (activeIndex == currentActiveTable.numIndivSpeciesB_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.numIndivSpeciesB);
            }
            else if (activeIndex == currentActiveTable.numIndivSpeciesC_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.numIndivSpeciesC);
            }
            else if (activeIndex == currentActiveTable.numQuadStudiedAndDensityA_PageIndex)
            {
                currentActiveTable.isNumQuadStudiedRevealed = true;
                SetTMPsState(currentActiveTable.numQuadStudiedTmps, true, true);
                ShowMessage(currentActiveTable.numQuadStudied_Msg);
                EnableDropdown(currentActiveTable, currentActiveTable.densitySpeciesA);
            }
            else if (activeIndex == currentActiveTable.densitySpeciesB_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.densitySpeciesB);
            }
            else if (activeIndex == currentActiveTable.densitySpeciesC_PageIndex)
            {
                EnableDropdown(currentActiveTable, currentActiveTable.densitySpeciesC);
            }
        }

        private void OnDropdownValueChanged(TableData table, TMP_Dropdown dropdown, int selectedIndex)
        {
            if (selectedIndex >= 0 && dropdown != null)
            {
                table.userSelectedDropdowns.Add(dropdown);

                if (dropdown.captionText != null && dropdown.options.Count > selectedIndex)
                {
                    dropdown.captionText.text = dropdown.options[selectedIndex].text;
                }

                if (table.correctAnswers.TryGetValue(dropdown, out int expectedCorrectIndex))
                {
                    bool isCorrect = (selectedIndex == expectedCorrectIndex);

                    if (table.feedbackImages.TryGetValue(dropdown, out Image feedbackImg) && feedbackImg != null)
                    {
                        feedbackImg.sprite = isCorrect ? correctSprite : wrongSprite;
                        feedbackImg.gameObject.SetActive(true);
                    }

                    PlayFeedbackSound(isCorrect);

                    if (isCorrect)
                    {
                        Debug.Log($"[{table.tableName}] Correct selection on dropdown '{dropdown.name}' (Index: {selectedIndex}).");
                        table.correctlyAnsweredDropdowns.Add(dropdown);
                        dropdown.interactable = false;
                        
                        // Hide message panel on correct selection
                        HideMessage();
                        UnlockPageNavigation();
                    }
                    else
                    {
                        Debug.LogWarning($"[{table.tableName}] Incorrect selection on '{dropdown.name}'. Selected: {selectedIndex}, Expected: {expectedCorrectIndex}");
                        
                        // Show message panel with specific wrong message for this dropdown
                        if (table.wrongMessages.TryGetValue(dropdown, out string wrongMsg))
                        {
                            ShowMessage(wrongMsg);
                        }
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

        private void SetPanelActiveState(TableData table, bool isActive)
        {
            if (table.observationTablePanel != null)
            {
                table.observationTablePanel.SetActive(isActive);
            }

            if (table.panelCanvasGroup != null)
            {
                table.panelCanvasGroup.blocksRaycasts = isActive;
            }
        }

        private void ResetAllControls(TableData table)
        {
            foreach (var dropdown in table.allDropdowns)
            {
                DisableDropdown(dropdown);
            }

            HideAllFeedbackImages(table);
            HideMessage();

            SetTMPsState(table.quad2Tmps, table.isQuad2Revealed, false);
            SetTMPsState(table.quad3Tmps, table.isQuad3Revealed, false);
            SetTMPsState(table.numQuadStudiedTmps, table.isNumQuadStudiedRevealed, false);
        }

        private void EnableDropdown(TableData table, TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;

            dropdown.gameObject.SetActive(true);

            // Highlight active dropdown text with activeHighlightColor (Yellow)
            if (dropdown.captionText != null)
            {
                dropdown.captionText.color = activeHighlightColor;
            }

            if (table.correctlyAnsweredDropdowns.Contains(dropdown))
            {
                dropdown.interactable = false;
                UnlockPageNavigation();
            }
            else
            {
                dropdown.interactable = true;

                if (!table.userSelectedDropdowns.Contains(dropdown))
                {
                    ResetDropdownState(table, dropdown);
                }
            }
        }

        private void DisableDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                dropdown.interactable = false;
                if (dropdown.captionText != null)
                {
                    dropdown.captionText.color = defaultColor;
                }
            }
        }

        private void SetTMPsState(List<GameObject> tmps, bool state, bool isHighlighted = false)
        {
            if (tmps == null) return;
            foreach (var tmp in tmps)
            {
                if (tmp != null)
                {
                    tmp.SetActive(state);
                    TMP_Text textComp = tmp.GetComponent<TMP_Text>();
                    if (textComp != null)
                    {
                        textComp.color = isHighlighted ? activeHighlightColor : defaultColor;
                    }
                }
            }
        }
    }
}