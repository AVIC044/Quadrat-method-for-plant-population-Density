using UnityEngine;
using UnityEngine.UI;

public class PageNavigationAudio : MonoBehaviour
{
    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip nextClip;
    [SerializeField] private AudioClip previousClip;

    private void OnEnable()
    {
        if (nextButton)
        {
            nextButton.onClick.RemoveListener(PlayNextSound);
            nextButton.onClick.AddListener(PlayNextSound);
        }

        if (previousButton)
        {
            previousButton.onClick.RemoveListener(PlayPreviousSound);
            previousButton.onClick.AddListener(PlayPreviousSound);
        }
    }

    private void OnDisable()
    {
        if (nextButton)
            nextButton.onClick.RemoveListener(PlayNextSound);

        if (previousButton)
            previousButton.onClick.RemoveListener(PlayPreviousSound);
    }

    private void PlayNextSound()
    {
        if (audioSource != null && nextClip != null)
        {
            audioSource.PlayOneShot(nextClip);
        }
    }

    private void PlayPreviousSound()
    {
        if (audioSource != null && previousClip != null)
        {
            audioSource.PlayOneShot(previousClip);
        }
    }
}