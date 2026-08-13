using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObservationTableController : MonoBehaviour
{
    [Header("Table Panel Reference")]
    [SerializeField] private GameObject tablePanel;

    [Header("Observation Table Toggle Button")]
    [SerializeField] private Button toggleTableButton;

    [System.Serializable]
    public class SpeciesRow
    {
        [Header("Identity")]
        public string speciesName;

        [Header("Quadrat I - Dropdown")]
        public TMP_Dropdown quadrat1Dropdown;
        public int quadrat1CorrectIndex;
        public GameObject quadrat1CheckIcon;
        public GameObject quadrat1CrossIcon;

        [Header("Quadrat II - Auto-fill Text")]
        public TMP_Text quadrat2Text;
        public string quadrat2Value;

        [Header("Quadrat III - Auto-fill Text")]
        public TMP_Text quadrat3Text;
        public string quadrat3Value;

        [Header("Number of Individuals (S) - Dropdown")]
        public TMP_Dropdown sDropdown;
        public int sCorrectIndex;
        public GameObject sCheckIcon;
        public GameObject sCrossIcon;

        [Header("Number of Quadrat Studied (Q) - Auto-fill Text")]
        public TMP_Text qText;
        public string qValue;

        [Header("Density (D) - Dropdown")]
        public TMP_Dropdown densityDropdown;
        public int densityCorrectIndex;
        public GameObject densityCheckIcon;
        public GameObject densityCrossIcon;
    }

    [Header("Species Rows")]
    [SerializeField]
    private SpeciesRow speciesA = new SpeciesRow { speciesName = "A" };

    [SerializeField]
    private SpeciesRow speciesB = new SpeciesRow { speciesName = "B" };

    [SerializeField]
    private SpeciesRow speciesC = new SpeciesRow { speciesName = "C" };


    // =========================================================
    // PAGE INDEXES
    // =========================================================
    //
    // Your PageNavigationController uses 0-based indexes.
    //
    // Page 14 = index 13
    // Page 15 = index 14
    // etc.
    // =========================================================

    private const int PAGE_14 = 13;
    private const int PAGE_15 = 14;
    private const int PAGE_16 = 15;
    private const int PAGE_17 = 16;
    private const int PAGE_18 = 17;

    // Page 19 = index 18
    private const int PAGE_19 = 18;

    private const int PAGE_20 = 19;
    private const int PAGE_21 = 20;
    private const int PAGE_22 = 21;
    private const int PAGE_23 = 22;
    private const int PAGE_24 = 23;
    private const int PAGE_25 = 24;

    // Page 26 = index 25
    private const int PAGE_26 = 25;

    // Page 27 = index 26
    private const int PAGE_27 = 26;


    // =========================================================
    // RUNTIME DROPDOWN STATE
    // =========================================================

    private class DropdownState
    {
        public TMP_Dropdown dropdown;

        public int correctIndex;

        public GameObject checkIcon;
        public GameObject crossIcon;

        public bool hasAnswered;
        public bool wasCorrect;
    }

    private readonly List<DropdownState> dropdownStates =
        new List<DropdownState>();

    private int currentPageIndex = -1;

    private DropdownState currentRequiredDropdown;


    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += HandlePageChanged;

        RegisterDropdownListeners();

        if (toggleTableButton != null)
        {
            toggleTableButton.onClick.AddListener(ToggleTablePanel);
        }
    }


    private void Start()
    {
        InitializeTable();

        if (PageNavigationController.Instance != null)
        {
            HandlePageChanged(PageNavigationController.CurrentIndex);
        }
    }


    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= HandlePageChanged;

        UnregisterDropdownListeners();

        if (toggleTableButton != null)
        {
            toggleTableButton.onClick.RemoveListener(ToggleTablePanel);
        }
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeTable()
    {
        if (tablePanel != null)
        {
            tablePanel.SetActive(false);
        }

        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = false;
        }

        SetAllDropdownsInteractable(false);

        ClearAllIcons();

        ClearAutoFillTexts();
    }


    // =========================================================
    // PAGE CHANGED
    // =========================================================

    private void HandlePageChanged(int pageIndex)
    {
        currentPageIndex = pageIndex;

        // -----------------------------------------------------
        // IMPORTANT:
        // Clear Q/Q2/Q3 whenever we change page.
        // They are shown again only on their required pages.
        // -----------------------------------------------------
        ClearAutoFillTexts();

        // Close table whenever we move to another page.
        CloseTablePanel();

        // No dropdown is required until we determine
        // what this page needs.
        currentRequiredDropdown = null;


        switch (pageIndex)
        {
            // =================================================
            // PAGE 14
            // Species A → Quadrat 1
            // =================================================

            case PAGE_14:

                PrepareDropdownPage(
                    speciesA.quadrat1Dropdown,
                    speciesA.quadrat1CheckIcon,
                    speciesA.quadrat1CrossIcon
                );

                break;


            // =================================================
            // PAGE 15
            // Species B → Quadrat 1
            // =================================================

            case PAGE_15:

                PrepareDropdownPage(
                    speciesB.quadrat1Dropdown,
                    speciesB.quadrat1CheckIcon,
                    speciesB.quadrat1CrossIcon
                );

                break;


            // =================================================
            // PAGE 16
            // Species C → Quadrat 1
            // =================================================

            case PAGE_16:

                PrepareDropdownPage(
                    speciesC.quadrat1Dropdown,
                    speciesC.quadrat1CheckIcon,
                    speciesC.quadrat1CrossIcon
                );

                break;


            // =================================================
            // PAGE 17
            // Quadrat 2 predefined
            // =================================================

            case PAGE_17:

                PrepareQuadrat2Page();

                break;


            // =================================================
            // PAGE 18
            // Quadrat 3 predefined
            // =================================================

            case PAGE_18:

                PrepareQuadrat3Page();

                break;


            // =================================================
            // PAGE 19
            // Existing MCQ
            // Observation table does nothing
            // =================================================

            case PAGE_19:

                DisableTableForPage();

                break;


            // =================================================
            // PAGE 20
            // Species A → S
            // =================================================

            case PAGE_20:

                PrepareDropdownPage(
                    speciesA.sDropdown,
                    speciesA.sCheckIcon,
                    speciesA.sCrossIcon
                );

                break;


            // =================================================
            // PAGE 21
            // Species B → S
            // =================================================

            case PAGE_21:

                PrepareDropdownPage(
                    speciesB.sDropdown,
                    speciesB.sCheckIcon,
                    speciesB.sCrossIcon
                );

                break;


            // =================================================
            // PAGE 22
            // Species C → S
            // =================================================

            case PAGE_22:

                PrepareDropdownPage(
                    speciesC.sDropdown,
                    speciesC.sCheckIcon,
                    speciesC.sCrossIcon
                );

                break;


            // =================================================
            // PAGE 23
            // Q predefined
            // + Species A → Density
            // =================================================

            case PAGE_23:

                PreparePage23();

                break;


            // =================================================
            // PAGE 24
            // Species B → Density
            // =================================================

            case PAGE_24:

                PrepareDropdownPage(
                    speciesB.densityDropdown,
                    speciesB.densityCheckIcon,
                    speciesB.densityCrossIcon
                );

                break;


            // =================================================
            // PAGE 25
            // Species C → Density
            // =================================================

            case PAGE_25:

                PrepareDropdownPage(
                    speciesC.densityDropdown,
                    speciesC.densityCheckIcon,
                    speciesC.densityCrossIcon
                );

                break;


            // =================================================
            // PAGE 26
            // Animation / other system
            // =================================================

            case PAGE_26:

                DisableTableForPage();

                break;


            // =================================================
            // PAGE 27
            // MCQ / animation / other system
            // =================================================

            case PAGE_27:

                DisableTableForPage();

                break;


            // =================================================
            // ALL OTHER PAGES
            // =================================================

            default:

                DisableTableForPage();

                break;
        }
    }


    // =========================================================
    // PREPARE NORMAL DROPDOWN PAGE
    // =========================================================

    private void PrepareDropdownPage(
        TMP_Dropdown requiredDropdown,
        GameObject checkIcon,
        GameObject crossIcon)
    {
        // Observation Table button can be clicked.
        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = true;
        }

        // Only the required dropdown can eventually be used.
        SetAllDropdownsInteractable(false);

        // Find its saved state.
        currentRequiredDropdown =
            FindDropdownState(requiredDropdown);

        if (currentRequiredDropdown == null)
        {
            Debug.LogWarning(
                "ObservationTableController: Required dropdown was not registered."
            );

            return;
        }

        // Hide visual feedback when entering/revisiting page.
        HideIcon(checkIcon);
        HideIcon(crossIcon);

        // -----------------------------------------------------
        // If the student already answered this page correctly
        // before, Next remains unlocked.
        // -----------------------------------------------------

        if (currentRequiredDropdown.hasAnswered &&
            currentRequiredDropdown.wasCorrect)
        {
            PageNavigationController.RequestNavigationUnlock();
        }
    }


    // =========================================================
    // PAGE 17
    // QUADRAT 2
    // =========================================================

    private void PrepareQuadrat2Page()
    {
        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = true;
        }

        SetAllDropdownsInteractable(false);

        // Show predefined Q2 values.
        SetText(
            speciesA.quadrat2Text,
            speciesA.quadrat2Value
        );

        SetText(
            speciesB.quadrat2Text,
            speciesB.quadrat2Value
        );

        SetText(
            speciesC.quadrat2Text,
            speciesC.quadrat2Value
        );
    }


    // =========================================================
    // PAGE 18
    // QUADRAT 3
    // =========================================================

    private void PrepareQuadrat3Page()
    {
        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = true;
        }

        SetAllDropdownsInteractable(false);

        // Show predefined Q3 values.
        SetText(
            speciesA.quadrat3Text,
            speciesA.quadrat3Value
        );

        SetText(
            speciesB.quadrat3Text,
            speciesB.quadrat3Value
        );

        SetText(
            speciesC.quadrat3Text,
            speciesC.quadrat3Value
        );
    }


    // =========================================================
    // PAGE 23
    // Q PREDEFINED + SPECIES A DENSITY
    // =========================================================

    private void PreparePage23()
    {
        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = true;
        }

        SetAllDropdownsInteractable(false);

        // Show predefined Q values.
        SetText(
            speciesA.qText,
            speciesA.qValue
        );

        SetText(
            speciesB.qText,
            speciesB.qValue
        );

        SetText(
            speciesC.qText,
            speciesC.qValue
        );

        // Only Species A Density is required.
        currentRequiredDropdown =
            FindDropdownState(speciesA.densityDropdown);

        if (currentRequiredDropdown == null)
        {
            Debug.LogWarning(
                "ObservationTableController: Species A Density dropdown not found."
            );

            return;
        }

        HideIcon(speciesA.densityCheckIcon);
        HideIcon(speciesA.densityCrossIcon);

        if (currentRequiredDropdown.hasAnswered &&
            currentRequiredDropdown.wasCorrect)
        {
            PageNavigationController.RequestNavigationUnlock();
        }
    }


    // =========================================================
    // DISABLE TABLE
    // =========================================================

    private void DisableTableForPage()
    {
        if (toggleTableButton != null)
        {
            toggleTableButton.interactable = false;
        }

        SetAllDropdownsInteractable(false);

        currentRequiredDropdown = null;

        if (tablePanel != null)
        {
            tablePanel.SetActive(false);
        }

        ClearAllIcons();
    }


    // =========================================================
    // TABLE BUTTON
    // =========================================================

    public void ToggleTablePanel()
    {
        if (tablePanel == null)
        {
            Debug.LogWarning(
                "ObservationTableController: Table Panel Reference is missing."
            );

            return;
        }

        bool shouldOpen = !tablePanel.activeSelf;

        tablePanel.SetActive(shouldOpen);

        if (shouldOpen)
        {
            OpenTablePanel();
        }
        else
        {
            CloseTablePanelOnly();
        }
    }


    // =========================================================
    // OPEN TABLE
    // =========================================================

    private void OpenTablePanel()
    {
        // Start with everything disabled.
        SetAllDropdownsInteractable(false);

        // -----------------------------------------------------
        // Pages 17 and 18:
        // They contain predefined values.
        // No dropdown is required.
        // Opening table completes the page.
        // -----------------------------------------------------

        if (currentPageIndex == PAGE_17 ||
            currentPageIndex == PAGE_18)
        {
            PageNavigationController.RequestNavigationUnlock();

            return;
        }

        // -----------------------------------------------------
        // Normal dropdown question
        // -----------------------------------------------------

        if (currentRequiredDropdown == null)
        {
            return;
        }

        // Enable ONLY the required dropdown.
        if (currentRequiredDropdown.dropdown != null)
        {
            currentRequiredDropdown.dropdown.interactable = true;
        }

        // If previously answered correctly,
        // keep Next unlocked.
        if (currentRequiredDropdown.hasAnswered &&
            currentRequiredDropdown.wasCorrect)
        {
            PageNavigationController.RequestNavigationUnlock();
        }
    }


    // =========================================================
    // PUBLIC OPEN METHOD
    // =========================================================

    public void OpenTable()
    {
        if (tablePanel == null)
        {
            return;
        }

        tablePanel.SetActive(true);

        OpenTablePanel();
    }


    // =========================================================
    // CLOSE TABLE WHEN MOVING PAGE
    // =========================================================

    private void CloseTablePanel()
    {
        if (tablePanel != null)
        {
            tablePanel.SetActive(false);
        }

        SetAllDropdownsInteractable(false);

        // IMPORTANT:
        //
        // We DO NOT reset dropdown values.
        // We DO NOT reset answer states.
        //
        // This is what allows the student to return later
        // and still see their previous selection.
        //
        // Only the check/cross icons are hidden.
        ClearAllIcons();
    }


    // =========================================================
    // CLOSE TABLE ONLY
    // =========================================================

    private void CloseTablePanelOnly()
    {
        if (tablePanel != null)
        {
            tablePanel.SetActive(false);
        }

        SetAllDropdownsInteractable(false);

        ClearAllIcons();
    }


    // =========================================================
    // REGISTER DROPDOWNS
    // =========================================================

    private void RegisterDropdownListeners()
    {
        dropdownStates.Clear();

        // ---------------------------------------------
        // Species A - Quadrat 1
        // ---------------------------------------------

        RegisterDropdown(
            speciesA.quadrat1Dropdown,
            speciesA.quadrat1CorrectIndex,
            speciesA.quadrat1CheckIcon,
            speciesA.quadrat1CrossIcon
        );

        // ---------------------------------------------
        // Species B - Quadrat 1
        // ---------------------------------------------

        RegisterDropdown(
            speciesB.quadrat1Dropdown,
            speciesB.quadrat1CorrectIndex,
            speciesB.quadrat1CheckIcon,
            speciesB.quadrat1CrossIcon
        );

        // ---------------------------------------------
        // Species C - Quadrat 1
        // ---------------------------------------------

        RegisterDropdown(
            speciesC.quadrat1Dropdown,
            speciesC.quadrat1CorrectIndex,
            speciesC.quadrat1CheckIcon,
            speciesC.quadrat1CrossIcon
        );

        // ---------------------------------------------
        // Species A - S
        // ---------------------------------------------

        RegisterDropdown(
            speciesA.sDropdown,
            speciesA.sCorrectIndex,
            speciesA.sCheckIcon,
            speciesA.sCrossIcon
        );

        // ---------------------------------------------
        // Species B - S
        // ---------------------------------------------

        RegisterDropdown(
            speciesB.sDropdown,
            speciesB.sCorrectIndex,
            speciesB.sCheckIcon,
            speciesB.sCrossIcon
        );

        // ---------------------------------------------
        // Species C - S
        // ---------------------------------------------

        RegisterDropdown(
            speciesC.sDropdown,
            speciesC.sCorrectIndex,
            speciesC.sCheckIcon,
            speciesC.sCrossIcon
        );

        // ---------------------------------------------
        // Species A - Density
        // ---------------------------------------------

        RegisterDropdown(
            speciesA.densityDropdown,
            speciesA.densityCorrectIndex,
            speciesA.densityCheckIcon,
            speciesA.densityCrossIcon
        );

        // ---------------------------------------------
        // Species B - Density
        // ---------------------------------------------

        RegisterDropdown(
            speciesB.densityDropdown,
            speciesB.densityCorrectIndex,
            speciesB.densityCheckIcon,
            speciesB.densityCrossIcon
        );

        // ---------------------------------------------
        // Species C - Density
        // ---------------------------------------------

        RegisterDropdown(
            speciesC.densityDropdown,
            speciesC.densityCorrectIndex,
            speciesC.densityCheckIcon,
            speciesC.densityCrossIcon
        );
    }


    // =========================================================
    // REGISTER ONE DROPDOWN
    // =========================================================

    private void RegisterDropdown(
        TMP_Dropdown dropdown,
        int correctIndex,
        GameObject checkIcon,
        GameObject crossIcon)
    {
        if (dropdown == null)
        {
            return;
        }

        DropdownState state = new DropdownState
        {
            dropdown = dropdown,
            correctIndex = correctIndex,
            checkIcon = checkIcon,
            crossIcon = crossIcon,
            hasAnswered = false,
            wasCorrect = false
        };

        dropdownStates.Add(state);

        dropdown.onValueChanged.AddListener(
            (selectedIndex) =>
            {
                HandleDropdownChanged(
                    state,
                    selectedIndex
                );
            }
        );
    }


    // =========================================================
    // DROPDOWN ANSWER
    // =========================================================

    private void HandleDropdownChanged(
        DropdownState state,
        int selectedIndex)
    {
        // -----------------------------------------------------
        // VERY IMPORTANT:
        // Ignore every dropdown except the one required
        // on the current page.
        // -----------------------------------------------------

        if (state != currentRequiredDropdown)
        {
            return;
        }

        state.hasAnswered = true;

        // Compare dropdown OPTION INDEX,
        // not the displayed text.
        state.wasCorrect =
            selectedIndex == state.correctIndex;

        if (state.wasCorrect)
        {
            // Correct answer
            ShowCorrectIcon(state);

            // Unlock Next.
            PageNavigationController.RequestNavigationUnlock();
        }
        else
        {
            // Wrong answer.
            ShowWrongIcon(state);

            // Do NOT unlock Next.
        }
    }


    // =========================================================
    // ENABLE / DISABLE DROPDOWNS
    // =========================================================

    private void SetAllDropdownsInteractable(bool interactable)
    {
        foreach (DropdownState state in dropdownStates)
        {
            if (state.dropdown != null)
            {
                state.dropdown.interactable = interactable;
            }
        }
    }


    // =========================================================
    // FIND DROPDOWN STATE
    // =========================================================

    private DropdownState FindDropdownState(
        TMP_Dropdown dropdown)
    {
        if (dropdown == null)
        {
            return null;
        }

        foreach (DropdownState state in dropdownStates)
        {
            if (state.dropdown == dropdown)
            {
                return state;
            }
        }

        return null;
    }


    // =========================================================
    // CORRECT / WRONG ICONS
    // =========================================================

    private void ShowCorrectIcon(DropdownState state)
    {
        if (state.checkIcon != null)
        {
            state.checkIcon.SetActive(true);
        }

        if (state.crossIcon != null)
        {
            state.crossIcon.SetActive(false);
        }
    }


    private void ShowWrongIcon(DropdownState state)
    {
        if (state.checkIcon != null)
        {
            state.checkIcon.SetActive(false);
        }

        if (state.crossIcon != null)
        {
            state.crossIcon.SetActive(true);
        }
    }


    private void HideIcon(GameObject icon)
    {
        if (icon != null)
        {
            icon.SetActive(false);
        }
    }


    private void ClearAllIcons()
    {
        foreach (DropdownState state in dropdownStates)
        {
            HideIcon(state.checkIcon);
            HideIcon(state.crossIcon);
        }
    }


    // =========================================================
    // AUTO-FILL TEXT
    // =========================================================

    private void ClearAutoFillTexts()
    {
        // Species A
        SetText(speciesA.quadrat2Text, "");
        SetText(speciesA.quadrat3Text, "");
        SetText(speciesA.qText, "");

        // Species B
        SetText(speciesB.quadrat2Text, "");
        SetText(speciesB.quadrat3Text, "");
        SetText(speciesB.qText, "");

        // Species C
        SetText(speciesC.quadrat2Text, "");
        SetText(speciesC.quadrat3Text, "");
        SetText(speciesC.qText, "");
    }


    private void SetText(
        TMP_Text text,
        string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void UnregisterDropdownListeners()
    {
        foreach (DropdownState state in dropdownStates)
        {
            if (state.dropdown != null)
            {
                state.dropdown.onValueChanged.RemoveAllListeners();
            }
        }

        dropdownStates.Clear();
    }
}