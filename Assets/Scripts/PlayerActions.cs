using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference tauntAction;
    [SerializeField] private InputActionReference toggleCombatAction;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float rotationSpeed = 12f;
    [Tooltip("How quickly the player reaches target speed. Higher = snappier.")]
    [SerializeField] private float acceleration = 12f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera (optional)")]
    [Tooltip("If set, movement is relative to this camera. Leave empty to use Camera.main.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Optional. Lets the controls change depending on the camera view. Found automatically if empty.")]
    [SerializeField] private CameraViewSwitcher cameraViews;

    [Header("Animation")]
    [Tooltip("How long movement is locked after each action (seconds). Match your clip lengths.")]
    [SerializeField] private float attackLockTime = 0.8f;
    [SerializeField] private float hitLockTime = 0.5f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int InCombatHash = Animator.StringToHash("InCombat");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int GetHitHash = Animator.StringToHash("GetHit");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int TauntHash = Animator.StringToHash("Taunt");
    private static readonly int DizzyHash = Animator.StringToHash("Dizzy");
    private static readonly int SenseSomethingHash = Animator.StringToHash("SenseSomething");
    private static readonly int VictoryHash = Animator.StringToHash("Victory");

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool isDead;
    private bool isDizzy;
    private bool isTaunting;
    private bool inCombat;
    private float lockTimer;
    private Animator animator;
    private Coroutine stunRoutine;

    private bool CanAct => !isDead && !isDizzy && lockTimer <= 0f;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraViews == null)
            cameraViews = GetComponent<CameraViewSwitcher>();
    }

    private void OnEnable()
    {
        Enable(moveAction);
        Enable(jumpAction);
        Enable(sprintAction);
        Enable(attackAction);
        Enable(tauntAction);
        Enable(toggleCombatAction);
    }

    private void OnDisable()
    {
        Disable(moveAction);
        Disable(jumpAction);
        Disable(sprintAction);
        Disable(attackAction);
        Disable(tauntAction);
        Disable(toggleCombatAction);
    }

    private void Update()
    {
        if (lockTimer > 0f)
            lockTimer -= Time.deltaTime;

        HandleActionInput();
        HandleHorizontalMovement();
        HandleVerticalMovement();

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        UpdateAnimator();
    }

    // ------------------------------------------------------------------
    // Movement
    // ------------------------------------------------------------------

    private void HandleHorizontalMovement()
    {
        Vector2 input = CanAct ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 moveDir = GetMoveDirection(input);

        bool sprinting = CanAct && sprintAction != null && sprintAction.action.IsPressed();
        float targetSpeed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = moveDir * targetSpeed;
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            acceleration * Time.deltaTime * targetSpeed);

        bool firstPerson = cameraViews != null && cameraViews.Mode == CameraViewMode.FirstPerson;

        if (firstPerson)
        {
            if (!isDead)
                transform.rotation = Quaternion.Euler(0f, cameraViews.Yaw, 0f);
        }
        else if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleVerticalMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (CanAct && jumpAction != null && controller.isGrounded
            && jumpAction.action.WasPressedThisFrame())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraViews != null && cameraViews.Mode != CameraViewMode.Original)
        {
            // Third / first person: movement follows the mouse-controlled direction
            Quaternion yawRotation = Quaternion.Euler(0f, cameraViews.Yaw, 0f);
            forward = yawRotation * Vector3.forward;
            right = yawRotation * Vector3.right;
        }
        else if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 dir = forward * input.y + right * input.x;
        return Vector3.ClampMagnitude(dir, 1f);
    }

    // ------------------------------------------------------------------
    // Actions (attack, taunt, combat stance)
    // ------------------------------------------------------------------

    private void HandleActionInput()
    {
        if (isDead) return;

        // Any other action (moving, jumping, attacking...) ends the taunt
        if (isTaunting && WantsToCancelTaunt())
            StopTaunt();

        if (toggleCombatAction != null && toggleCombatAction.action.WasPressedThisFrame())
            SetCombat(!inCombat);

        if (!CanAct) return;

        if (attackAction != null && attackAction.action.WasPressedThisFrame())
            PerformAttack();
        else if (tauntAction != null && tauntAction.action.WasPressedThisFrame())
            PlayTaunt();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(SpeedHash, horizontalVelocity.magnitude, 0.1f, Time.deltaTime);
    }

    // ------------------------------------------------------------------
    // Public API: call these from other scripts (enemies, triggers, UI...)
    // ------------------------------------------------------------------

    public void PerformAttack()
    {
        if (!CanAct) return;

        SetCombat(true);

        animator.SetTrigger(AttackHash);

        lockTimer = attackLockTime;
    }

    /// <summary>Starts the taunt. It loops until another action cancels it.</summary>
    public void PlayTaunt()
    {
        if (!CanAct || isTaunting) return;

        isTaunting = true;
        animator.SetBool(TauntHash, true);
    }

    public void StopTaunt()
    {
        if (!isTaunting) return;

        isTaunting = false;
        animator.SetBool(TauntHash, false);
    }

    private bool WantsToCancelTaunt()
    {
        if (moveAction.action.ReadValue<Vector2>().sqrMagnitude > 0.01f)
            return true;

        return WasPressed(jumpAction)
            || WasPressed(attackAction)
            || WasPressed(toggleCombatAction);
    }

    private static bool WasPressed(InputActionReference reference)
    {
        return reference != null && reference.action.WasPressedThisFrame();
    }

    public void SetCombat(bool value)
    {
        inCombat = value;
        if (animator != null)
            animator.SetBool(InCombatHash, inCombat);
    }

    public void TakeHit()
    {
        if (isDead) return;
        StopTaunt();
        animator.SetTrigger(GetHitHash);
        lockTimer = hitLockTime;
    }

    public void Stun(float duration)
    {
        if (isDead) return;

        StopTaunt();
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isDizzy = true;
        animator.SetBool(DizzyHash, true);

        yield return new WaitForSeconds(duration);

        isDizzy = false;
        animator.SetBool(DizzyHash, false);
        stunRoutine = null;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isDizzy = false;
        StopTaunt();
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }

        animator.SetBool(DizzyHash, false);
        animator.SetTrigger(DieHash);
    }

    public void Victory(bool value)
    {
        if (isDead) return;
        if (value)
            StopTaunt();
        animator.SetBool(VictoryHash, value);
    }

    public void SenseSomething(bool value)
    {
        if (isDead) return;
        if (value)
            StopTaunt();
        animator.SetBool(SenseSomethingHash, value);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void Enable(InputActionReference reference)
    {
        if (reference != null) reference.action.Enable();
    }

    private static void Disable(InputActionReference reference)
    {
        if (reference != null) reference.action.Disable();
    }
}