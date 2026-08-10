using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PageOptionSelectionHandler : MonoBehaviour
{
    [Header("Page Settings")]
    [Tooltip("Page 6 = Index 5 | Page 7 = Index 6")]
    [SerializeField] private int targetPageIndex = 5;

    [Header("Buttons")]
    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;

    [Header("Child Indicator UI Images")]
    [SerializeField] private GameObject correctChildImage;
    [SerializeField] private GameObject wrongChildImage;

    [Header("Animation Settings")]
    [SerializeField] private float popDuration = 0.25f;

    private bool isAnswered = false;

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
        // 1. Hide both child UI images initially
        if (correctChildImage != null) correctChildImage.SetActive(false);
        if (wrongChildImage != null) wrongChildImage.SetActive(false);

        // 2. Attach button click listeners
        if (correctButton != null) correctButton.onClick.AddListener(OnCorrectSelected);
        if (wrongButton != null) wrongButton.onClick.AddListener(OnWrongSelected);

        // 3. Sync initial active state with current page index
        gameObject.SetActive(PageNavigationController.CurrentIndex == targetPageIndex);
    }

    private void HandlePageChanged(int newPageIndex)
    {
        // Automatically show/hide this slide based on current page index
        gameObject.SetActive(newPageIndex == targetPageIndex);
    }

    private void OnCorrectSelected()
    {
        if (isAnswered) return;
        isAnswered = true;

        DisableInteraction();

        // Reveal ONLY correct image
        StartCoroutine(PopAnimation(correctChildImage));

        // Unlock next button in PageNavigationController
        PageNavigationController.RequestNavigationUnlock();
    }

    private void OnWrongSelected()
    {
        if (isAnswered) return;
        isAnswered = true;

        DisableInteraction();

        // Reveal BOTH images
        StartCoroutine(PopAnimation(wrongChildImage));
        StartCoroutine(PopAnimation(correctChildImage));

        // Unlock next button in PageNavigationController
        PageNavigationController.RequestNavigationUnlock();
    }

    private void DisableInteraction()
    {
        if (correctButton != null) correctButton.interactable = false;
        if (wrongButton != null) wrongButton.interactable = false;
    }

    private IEnumerator PopAnimation(GameObject targetObject)
    {
        if (targetObject == null) yield break;

        targetObject.SetActive(true);
        Transform targetTransform = targetObject.transform;
        targetTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float scale = Mathf.Sin(t * Mathf.PI * 0.65f) * 1.2f;
            targetTransform.localScale = Vector3.one * Mathf.Clamp01(scale);

            yield return null;
        }

        targetTransform.localScale = Vector3.one;
    }
}