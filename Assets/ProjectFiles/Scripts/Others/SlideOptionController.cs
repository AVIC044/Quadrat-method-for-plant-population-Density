using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlideOptionController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;

    [Header("Child Indicator UI Images")]
    [SerializeField] private GameObject correctChildImage;
    [SerializeField] private GameObject wrongChildImage;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;

    [Header("Pop Animation Settings")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float delayBeforeCorrectPop = 0.3f;

    private bool isAnswered = false;

    private void OnEnable()
    {
        // 1. Reset selection state when enabled
        isAnswered = false;

        // 2. Hide indicators initially
        if (correctChildImage != null) correctChildImage.SetActive(false);
        if (wrongChildImage != null) wrongChildImage.SetActive(false);

        // 3. Reset button clickability and re-bind listeners
        if (correctButton != null)
        {
            correctButton.interactable = true;
            correctButton.onClick.RemoveListener(OnCorrectSelected);
            correctButton.onClick.AddListener(OnCorrectSelected);
        }

        if (wrongButton != null)
        {
            wrongButton.interactable = true;
            wrongButton.onClick.RemoveListener(OnWrongSelected);
            wrongButton.onClick.AddListener(OnWrongSelected);
        }
    }

    private void OnDisable()
    {
        if (correctButton != null) correctButton.onClick.RemoveListener(OnCorrectSelected);
        if (wrongButton != null) wrongButton.onClick.RemoveListener(OnWrongSelected);
    }

    private void OnCorrectSelected()
    {
        if (isAnswered) return;
        isAnswered = true;

        DisableInteraction();
        PlaySound(correctClip);

        // Pop ONLY correct image
        StartCoroutine(PopAnimation(correctChildImage));

        // Unlock next button in PageNavigationController
        PageNavigationController.RequestNavigationUnlock();
    }

    private void OnWrongSelected()
    {
        if (isAnswered) return;
        isAnswered = true;

        DisableInteraction();
        PlaySound(wrongClip);

        // Pop Wrong image, THEN automatically pop Correct image
        StartCoroutine(HandleWrongSelectionSequence());
    }

    private IEnumerator HandleWrongSelectionSequence()
    {
        // 1. Pop Wrong Image
        yield return StartCoroutine(PopAnimation(wrongChildImage));

        // 2. Brief delay before showing correct answer
        yield return new WaitForSeconds(delayBeforeCorrectPop);

        // 3. Auto-pop Correct Image
        yield return StartCoroutine(PopAnimation(correctChildImage));

        // 4. Unlock next button in PageNavigationController
        PageNavigationController.RequestNavigationUnlock();
    }

    private void DisableInteraction()
    {
        if (correctButton != null) correctButton.interactable = false;
        if (wrongButton != null) wrongButton.interactable = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
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

            // Elastic overshoot animation
            float scale = Mathf.Sin(t * Mathf.PI * 0.65f) * 1.2f;
            targetTransform.localScale = Vector3.one * Mathf.Clamp01(scale);

            yield return null;
        }

        targetTransform.localScale = Vector3.one;
    }
}