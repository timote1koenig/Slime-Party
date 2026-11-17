using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraViewMode
{
    Original,
    ThirdPerson,
    FirstPerson
}

public class CameraViewSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [Tooltip("The player (the object with PlayerMovement).")]
    [SerializeField] private Transform target;
    [Tooltip("Used for first person (empty object at eye height, child of the player).")]
    [SerializeField] private Transform headPoint;
    [Tooltip("Used for third person: only its distance from the player is used as the orbit distance.")]
    [SerializeField] private Transform ThirdPersonPoint;
    [Tooltip("Optional. Renderers of this object are hidden in first person (true first person).")]

    [Header("Input Actions")]
    [SerializeField] private InputActionReference lookAction;

    [Header("General")]
    [SerializeField] private CameraViewMode startMode = CameraViewMode.Original;
    [SerializeField] private float transitionTime = 0.35f;
    [SerializeField] private float lookSensitivity = 0.12f;
    [Tooltip("If on, the Original view follows the player. If off, the camera stays where it started.")]
    [SerializeField] private bool originalFollowsPlayer = false;

    [Header("Third Person")]
    [Tooltip("Height above the player's feet that the camera orbits and looks at.")]
    [SerializeField] private float thirdPersonLookHeight = 1.5f;
    [SerializeField] private float fallbackDistance = 4f;
    [SerializeField] private Vector2 thirdPersonPitchLimits = new Vector2(-20f, 60f);

    [Header("First Person")]
    [SerializeField] private float eyeHeightFallback = 1.6f;
    [SerializeField] private float firstPersonNearClip = 0.05f;
    [SerializeField] private Vector2 firstPersonPitchLimits = new Vector2(-80f, 80f);

    // Read by PlayerMovement so the controls can depend on the view
    public CameraViewMode Mode => mode;
    public float Yaw => yaw;

    private CameraViewMode mode;

    private Vector3 originalPosition;
    private Vector3 originalOffset;
    private Quaternion originalRotation;
    private float originalNearClip;
    private float thirdPersonDistance;

    private float yaw;
    private float pitch;

    private Vector3 fromPosition;
    private Quaternion fromRotation;
    private float blend = 1f;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        originalNearClip = cam.nearClipPlane;
    }

    private void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
    }

    private void Start()
    {
        originalPosition = cam.transform.position;
        originalOffset = cam.transform.position - target.position;
        originalRotation = cam.transform.rotation;

        thirdPersonDistance = fallbackDistance;
        if (ThirdPersonPoint != null)
        {
            Vector3 pivot = target.position + Vector3.up * thirdPersonLookHeight;
            float distance = Vector3.Distance(ThirdPersonPoint.position, pivot);
            if (distance > 0.5f) thirdPersonDistance = distance;
        }

        SetMode(startMode, instant: true);
    }

    private void Update()
    {
        if (mode != CameraViewMode.Original && lookAction != null)
        {
            Vector2 look = lookAction.action.ReadValue<Vector2>();
            yaw += look.x * lookSensitivity;
            pitch -= look.y * lookSensitivity;

            Vector2 limits = mode == CameraViewMode.FirstPerson
                ? firstPersonPitchLimits
                : thirdPersonPitchLimits;
            pitch = Mathf.Clamp(pitch, limits.x, limits.y);
        }
    }

    private void LateUpdate()
    {
        GetDesiredPose(out Vector3 position, out Quaternion rotation);

        if (blend < 1f)
        {
            blend = transitionTime <= 0f ? 1f : Mathf.Min(1f, blend + Time.deltaTime / transitionTime);
            float t = Mathf.SmoothStep(0f, 1f, blend);
            position = Vector3.Lerp(fromPosition, position, t);
            rotation = Quaternion.Slerp(fromRotation, rotation, t);
        }

        cam.transform.SetPositionAndRotation(position, rotation);
    }

    // ------------------------------------------------------------------

    public void SetMode(CameraViewMode newMode, bool instant)
    {
        mode = newMode;

        fromPosition = cam.transform.position;
        fromRotation = cam.transform.rotation;
        blend = instant ? 1f : 0f;

        if (mode != CameraViewMode.Original)
        {
            yaw = target.eulerAngles.y;
            pitch = mode == CameraViewMode.FirstPerson ? 0f : 15f;
        }

        bool firstPerson = mode == CameraViewMode.FirstPerson;
        cam.nearClipPlane = firstPerson ? firstPersonNearClip : originalNearClip;

        Cursor.lockState = mode == CameraViewMode.Original ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = mode == CameraViewMode.Original;
    }

    private void GetDesiredPose(out Vector3 position, out Quaternion rotation)
    {
        Quaternion lookRotation = Quaternion.Euler(pitch, yaw, 0f);

        switch (mode)
        {
            case CameraViewMode.ThirdPerson:
                Vector3 pivot = target.position + Vector3.up * thirdPersonLookHeight;
                position = pivot - lookRotation * Vector3.forward * thirdPersonDistance;
                rotation = lookRotation;
                break;

            case CameraViewMode.FirstPerson:
                position = headPoint != null
                    ? headPoint.position
                    : target.position + Vector3.up * eyeHeightFallback;
                rotation = lookRotation;
                break;

            default:
                position = originalFollowsPlayer
                    ? target.position + originalOffset
                    : originalPosition;
                rotation = originalRotation;
                break;
        }
    }
}