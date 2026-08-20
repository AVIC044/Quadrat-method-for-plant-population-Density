using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;


public class TouchOrClickEventRotation : MonoBehaviour
{
    [Header("Serialized References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform targetToRotate;
    [SerializeField] private float timecount = 1.0f;

    [Header("Rotation Settings")]
    public float angleOfRotation;
    public int currentSlideIndex;

    // ============================================================
    // ROTATION EVENTS
    // ============================================================

    [Header("Rotation Events")]

    public UnityEvent OnRotationStarted;
    public UnityEvent OnRotationCompleted;

    // ============================================================
    // CONDITIONAL ROTATION EVENTS
    // ============================================================

    [System.Serializable]
    public class ConditionalRotationEvent
    {
        [Header("Slide Condition")]
        [Tooltip("Event will trigger only when the current slide index matches this value.")]
        public int requiredSlideIndex;

        public UnityEvent onInvoked;

        [Header("Trigger Settings")]
        public bool allowMultipleTriggers = true;

        [HideInInspector]
        public bool hasTriggered;
    }

    [Header("Invoke When Slide Index Matches")]
    public List<ConditionalRotationEvent> conditionalRotationEvents =
        new List<ConditionalRotationEvent>();

    // ============================================================
    // INTERNAL
    // ============================================================

    private Coroutine rotationCoroutine;

    // ============================================================
    // ROTATION
    // ============================================================

    private IEnumerator RotateObjectRoutine(float angle)
    {
        Quaternion targetInitialRotation = targetToRotate.rotation;

        Quaternion targetFinalRotation =
            targetInitialRotation * Quaternion.Euler(0f, 0f, angle);

        float elapsedTime = 0.0f;

        OnRotationStarted?.Invoke();

        while (elapsedTime < timecount)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / timecount);

            targetToRotate.rotation =
                Quaternion.Lerp(
                    targetInitialRotation,
                    targetFinalRotation,
                    t
                );

            yield return null;
        }

        targetToRotate.rotation = targetFinalRotation;

        InvokeConditionalRotationEvents();

        OnRotationCompleted?.Invoke();

        rotationCoroutine = null;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    public void RotateObject()
    {
        if (rotationCoroutine != null)
            return;

        rotationCoroutine = StartCoroutine(
            RotateObjectRoutine(angleOfRotation)
        );
    }

    // ============================================================
    // CONDITIONAL EVENTS
    // ============================================================

    private void InvokeConditionalRotationEvents()
    {
        int currentIndex = currentSlideIndex;

        foreach (var entry in conditionalRotationEvents)
        {
            if (entry.requiredSlideIndex != currentIndex)
                continue;

            if (!entry.allowMultipleTriggers && entry.hasTriggered)
                continue;

            entry.hasTriggered = true;
            entry.onInvoked?.Invoke();
        }
    }

    public void ResetAllConditionalTriggers()
    {
        foreach (var entry in conditionalRotationEvents)
        {
            entry.hasTriggered = false;
        }
    }
}
