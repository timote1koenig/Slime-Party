using UnityEngine;
using System.Collections;

public class MazeCameraAnimator : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField, Range(0f, 50f)] private float orbitSpeed = 12f;
    [SerializeField, Range(0f, 400)] private float orbitDistance = 150f;
    [SerializeField, Range(0f, 100f)] private float orbitHeight = 45f;

    [Header("Transition")]
    [SerializeField, Range(0f, 10f)] private float defaultTransitionDuration = 2f;

    private Vector3 target;
    private bool isOrbiting;
    private float currentAngle;

    private void LateUpdate()
    {
        if (!isOrbiting)
            return;

        currentAngle += orbitSpeed * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(orbitHeight, currentAngle, 0f);

        Vector3 offset = rotation * Vector3.back * orbitDistance;

        transform.position = target + offset;

        transform.LookAt(target);
    }

    public void BeginOrbit(Vector3 center)
    {
        StopAllCoroutines();

        target = center;
        currentAngle = 0f;
        isOrbiting = true;

        Quaternion rotation = Quaternion.Euler(orbitHeight, currentAngle, 0f);

        transform.position = target + rotation * Vector3.back * orbitDistance;

        transform.LookAt(target);
    }

    public void StopOrbit()
    {
        isOrbiting = false;
    }

    public IEnumerator FinishOrbit(Transform destination, float duration = -1f)
    {
        if (destination == null)
        {
            StopOrbit();
            yield break;
        }

        if (duration < 0f)
            duration = defaultTransitionDuration;

        isOrbiting = false;

        transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        destination.GetPositionAndRotation(out Vector3 endPosition, out Quaternion endRotation);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            transform.SetPositionAndRotation(Vector3.Lerp(
                startPosition,
                endPosition,
                smoothT
            ), Quaternion.Slerp(
                startRotation,
                endRotation,
                smoothT
            ));
            yield return null;
        }

        transform.SetPositionAndRotation(endPosition, endRotation);
    }
}
