using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SlimeSlingshot : MonoBehaviour
{
    [Header("Launch")]
    public float launchAngle = 30f;
    public float powerPerPixel = 0.08f;
    public float maxPower = 30f;
    public float aimSensitivity = 0.2f;
    public float maxAimAngle = 45f;

    [Header("Shots")]
    public int maxShots = 3;
    public float stopSpeed = 0.3f;     // 이 속도보다 느리면 멈춘 걸로 봄
    public float stopWaitTime = 1f;    // 멈추고 이만큼 지나면 다음 슬라임
    public float maxFlightTime = 8f;   // 최대 비행 시간
    public float fallLimitY = -5f;     // 맵 밖으로 떨어진 기준

    public enum State { Ready, Aiming, Flying, Done }
    public State CurrentState { get; private set; } = State.Ready;
    public float AimYaw { get; private set; }
    public float CurrentPower { get; private set; }
    public int ShotsLeft { get; private set; }
    public bool CanShoot { get; set; } = true;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;
    Vector2 dragStart;
    float baseYaw;
    float flightTimer;
    float stillTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
        baseYaw = transform.eulerAngles.y;
        rb.isKinematic = true;
        ShotsLeft = maxShots;
    }

    void Update()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;

        switch (CurrentState)
        {
            case State.Ready:
                if (CanShoot && mouse != null && mouse.leftButton.wasPressedThisFrame)
                {
                    dragStart = mouse.position.ReadValue();
                    CurrentState = State.Aiming;
                }
                break;

            case State.Aiming:
                if (mouse == null) break;
                Vector2 drag = mouse.position.ReadValue() - dragStart;
                AimYaw = Mathf.Clamp(-drag.x * aimSensitivity, -maxAimAngle, maxAimAngle);
                CurrentPower = Mathf.Clamp(-drag.y * powerPerPixel, 0f, maxPower);
                transform.rotation = Quaternion.Euler(0f, baseYaw + AimYaw, 0f);

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    if (CurrentPower > 1f) Launch();
                    else CurrentState = State.Ready;
                }
                break;

            case State.Flying:
                flightTimer += Time.deltaTime;
                if (rb.linearVelocity.magnitude < stopSpeed) stillTimer += Time.deltaTime;
                else stillTimer = 0f;

                bool skip = kb != null && kb.rKey.wasPressedThisFrame;
                if (skip || stillTimer >= stopWaitTime || flightTimer >= maxFlightTime
                    || transform.position.y < fallLimitY)
                {
                    EndShot();
                }
                break;
        }
    }

    public Vector3 LaunchDirection =>
        Quaternion.Euler(-launchAngle, baseYaw + AimYaw, 0f) * Vector3.forward;

    void Launch()
    {
        rb.isKinematic = false;
        rb.linearVelocity = LaunchDirection * CurrentPower;
        ShotsLeft--;
        flightTimer = 0f;
        stillTimer = 0f;
        CurrentState = State.Flying;
    }

    void EndShot()
    {
        if (ShotsLeft > 0 && CanShoot) ResetSlime();
        else CurrentState = State.Done; // 다 썼거나 게임 끝
    }

    void ResetSlime()
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;
        transform.SetPositionAndRotation(startPos, startRot);
        AimYaw = 0f;
        CurrentPower = 0f;
        CurrentState = State.Ready;
    }
}