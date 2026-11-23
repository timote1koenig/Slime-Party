using UnityEngine;

public class KnockableBlock : MonoBehaviour
{
    public int points = 100;            // 이 블록 점수
    public float moveThreshold = 0.5f;
    public float tiltThreshold = 30f;

    public bool IsKnocked { get; private set; }
    Vector3 startPos;
    Renderer rend;

    void Start()
    {
        startPos = transform.position;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (IsKnocked) return;

        bool moved = Vector3.Distance(transform.position, startPos) > moveThreshold;
        bool tilted = Vector3.Angle(transform.up, Vector3.up) > tiltThreshold;

        if (moved || tilted)
        {
            IsKnocked = true;
            // 원래 색에서 어둡게 (나무는 진한 갈색, 돌은 진한 회색)
            rend.material.color = Color.Lerp(rend.material.color, Color.black, 0.5f);
        }
    }
}