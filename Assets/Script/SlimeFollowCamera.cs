using UnityEngine;

public class SlimeFollowCamera : MonoBehaviour
{
    public SlimeSlingshot slime;
    public float distance = 4f;     // 슬라임 뒤 거리
    public float height = 1.5f;     // 슬라임 위 높이
    public float followSmooth = 5f; // 따라가는 부드러움

    Rigidbody slimeRb;
    Vector3 flyDir = Vector3.forward;

    void Start()
    {
        if (slime != null) slimeRb = slime.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (slime == null) return;
        Vector3 target = slime.transform.position;

        if (slime.CurrentState == SlimeSlingshot.State.Flying || slime.CurrentState == SlimeSlingshot.State.Done)
        {
            Vector3 v = slimeRb.linearVelocity;
            v.y = 0f;
            if (v.sqrMagnitude > 0.5f) flyDir = v.normalized; // 날아가는 방향 뒤에 붙기
        }
        else
        {
            Vector3 f = slime.transform.forward;
            f.y = 0f;
            flyDir = f.normalized; // 조준 방향 뒤에 붙기
        }

        Vector3 desired = target - flyDir * distance + Vector3.up * height;
        transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
        transform.LookAt(target + flyDir * 3f + Vector3.up * 0.5f);
    }
}