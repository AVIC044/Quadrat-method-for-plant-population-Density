using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TouchOrClickEventRotation : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private Camera targetCamera;

    [Header("Rotation References")]
    [SerializeField] private Transform targetToRotate;
    [SerializeField] private float timecount = 1.0f;

    [Header("Slide / Index Settings")]
    [Tooltip("The slide index on which this object is allowed to rotate.")]
    [SerializeField] private int requiredSlideIndex;

    [Tooltip("The current slide index used for the built-in index check.")]
    public int currentSlideIndex;

    [Header("Input Behavior")]
    [Tooltip("If enabled, clicks/touches over UI elements will be ignored.")]
    [SerializeField] private bool ignoreUI = true;

    [Header("Rotation Settings")]
    public float angleOfRotation;

    [Header("Rotation Events")]
    public UnityEvent OnRotationStarted;
    public UnityEvent OnRotationCompleted;

    // ============================================================
    // INTERNAL
    // ============================================================

    private Collider cachedCollider;
    private Coroutine rotationCoroutine;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetToRotate == null)
            targetToRotate = transform;
    }

    // ============================================================
    // INPUT
    // ============================================================

    private void Update()
    {
        if (targetCamera == null)
            return;

        // -------- MOUSE --------
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ProcessPointer(Mouse.current.position.ReadValue());
        }

        // -------- TOUCH --------
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                ProcessPointer(touch.position.ReadValue());
            }
        }
    }

    private void ProcessPointer(Vector2 screenPosition)
    {
        if (ignoreUI &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (hit.collider != cachedCollider)
            return;

        // The index check is built into the rotation trigger.
        if (currentSlideIndex != requiredSlideIndex)
            return;

        RotateObject();
    }

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

        // Keep the public API safe as well: rotation can only start
        // on the required index, even if RotateObject() is called
        // from another UnityEvent.
        if (currentSlideIndex != requiredSlideIndex)
            return;

        rotationCoroutine = StartCoroutine(
            RotateObjectRoutine(angleOfRotation)
        );
    }
}