using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on the single, SHARED "Observation Table" reveal button (the
/// same button and table object are reused across many slides - only the
/// dropdown CONTENT inside the table changes per slide).
///
/// - The button is visible/clickable only while the current page NUMBER is
///   in "validPageNumbers" (hidden via CanvasGroup on every other page - NOT
///   via SetActive, since disabling this GameObject would also disable this
///   script and it could never hear about future page changes to re-show
///   itself).
/// - Each time you arrive on a NEW page in that list, the table defaults to
///   CLOSED - the player must press the button to reveal it (showing that
///   page's own dropdown content).
/// - If you navigate back to a page in the list that's ALREADY been
///   answered correctly (tracked via PageNavigationController.IsPageCompleted),
///   the table auto-opens immediately showing the correct answer - no
///   button press needed.
///
/// SETUP:
/// 1. Put this script on the single Observation Table button GameObject.
/// 2. Assign "tablePanel" to the single shared table/panel this button reveals.
/// 3. In "validPageNumbers", list every page NUMBER (1-based, matching what's
///    shown on screen) this shared button/table should appear on (e.g.
///    15, 16, 17, 18, 19 - any set of numbers, doesn't need to be contiguous).
/// 4. Assign "toggleButton" to this GameObject's own Button component
///    (or leave empty - it will auto-find one on this GameObject).
/// 5. A CanvasGroup is auto-added to this GameObject if missing - no manual
///    setup needed for that part.
/// </summary>
public class ObservationTableToggle : MonoBehaviour
{
    [Header("What this button controls")]
    [Tooltip("The single shared table/panel this button shows and hides.")]
    [SerializeField] private GameObject tablePanel;

    [Header("Page List Tracking")]
    [Tooltip("Every page NUMBER (1-based, matching what's shown on screen - page 1, page 2, etc.) this shared button/table should appear on. Doesn't need to be contiguous - list any set of numbers.")]
    [SerializeField] private List<int> validPageNumbers = new List<int>();

    [Header("Button (auto-found if left empty)")]
    [SerializeField] private Button toggleButton;

    private CanvasGroup canvasGroup;

    // Tracks which page the table was last opened/closed for, so we know
    // when the player has moved to a DIFFERENT page in the list (and should
    // therefore re-evaluate open/closed) versus just re-entering the same page.
    private int lastSeenPageIndex = int.MinValue;

    private void Awake()
    {
        if (toggleButton == null)
            toggleButton = GetComponent<Button>();


        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Table starts closed.
        if (tablePanel != null)
            tablePanel.SetActive(false);
    }

    void Start()
    {

        
        if (toggleButton != null)
            toggleButton.onClick.AddListener(OnToggleClicked);
    }
    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += HandlePageChanged;
    }

    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= HandlePageChanged;
    }

    /// <summary>Pressing the button flips the table's active state on/off.</summary>
    public void OnToggleClicked()
    {
        if (tablePanel == null) return;
        Debug.Log("Button Clicked");
        tablePanel.SetActive(!tablePanel.activeSelf);
    }

    /// <summary>
    /// Runs every time the page changes. Shows/hides the BUTTON (via
    /// CanvasGroup, not SetActive) based on whether the current page NUMBER
    /// is in validPageNumbers, and decides the table's state for whichever
    /// page we just landed on in that list.
    /// </summary>
    private void HandlePageChanged(int newIndex)
    {
        // PageNavigationController's currentIndex is 0-based internally, but
        // validPageNumbers is entered as 1-based (matching what's shown on
        // screen) - convert before comparing.
        int newPageNumber = newIndex + 1;

        bool inList = validPageNumbers.Contains(newPageNumber);

        // Show/hide the button visually and functionally, WITHOUT disabling
        // this GameObject (which would also disable this script).
        canvasGroup.alpha = inList ? 1f : 0f;
        canvasGroup.interactable = inList;
        canvasGroup.blocksRaycasts = inList;

        if (tablePanel == null) return;

        if (!inList)
        {
            // Left the list entirely - table resets closed for next time.
            tablePanel.SetActive(false);
            lastSeenPageIndex = int.MinValue;
            return;
        }

        if (newIndex == lastSeenPageIndex)
        {
            // Page didn't actually change (e.g. a redundant event) - leave
            // the table's current open/closed state exactly as the player
            // left it, don't force anything.
            return;
        }

        lastSeenPageIndex = newIndex;

        // Landed on a (possibly new) page in the list. If THIS page was
        // already completed before, auto-open the table showing the correct
        // answer. If not yet answered, keep it closed until pressed.
        bool alreadyCompleted = PageNavigationController.Instance != null
            && PageNavigationController.Instance.IsPageCompleted(newIndex);

        tablePanel.SetActive(alreadyCompleted);
    }
}