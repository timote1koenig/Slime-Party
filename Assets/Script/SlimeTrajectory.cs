using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlimeTrajectory : MonoBehaviour
{
    public SlimeSlingshot slingshot;
    public int pointCount = 40;      // 선을 이루는 점 개수
    public float timeStep = 0.08f;   // 점 사이 시간 간격

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (slingshot == null) slingshot = GetComponent<SlimeSlingshot>();

        line.useWorldSpace = true;
        line.startWidth = 0.15f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1f, 1f, 1f, 0.9f);
        line.endColor = new Color(1f, 1f, 1f, 0f); // 끝으로 갈수록 투명
        line.positionCount = 0;
    }

    void Update()
    {
        // 조준 중일 때만 보이기
        if (slingshot.CurrentState != SlimeSlingshot.State.Aiming || slingshot.CurrentPower < 1f)
        {
            line.positionCount = 0;
            return;
        }

        Vector3 start = transform.position;
        Vector3 velocity = slingshot.LaunchDirection * slingshot.CurrentPower;
        Vector3 gravity = Physics.gravity;

        line.positionCount = pointCount;
        Vector3 prev = start;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i * timeStep;
            Vector3 p = start + velocity * t + 0.5f * gravity * t * t;

            // 뭔가에 부딪히면 거기서 선 끊기
            if (i > 0 && Physics.Linecast(prev, p, out RaycastHit hit)
                && hit.collider.transform != transform)
            {
                line.positionCount = i + 1;
                line.SetPosition(i, hit.point);
                return;
            }

            line.SetPosition(i, p);
            prev = p;
        }
    }
}