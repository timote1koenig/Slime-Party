using UnityEngine;
using System.Collections;

public class BombBehavior : MonoBehaviour
{
    [SerializeField] private GameObject ExplosionParticleGameObject;
    [SerializeField] private float explosionTimerMin = 30f;
    [SerializeField] private float explosionTimerMax = 50f;

    private float explosionTimer;

    void Start()
    {
        explosionTimer = Random.Range(explosionTimerMin, explosionTimerMax);

        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(explosionTimer);

        Instantiate(ExplosionParticleGameObject, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
