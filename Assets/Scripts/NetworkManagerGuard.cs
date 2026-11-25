
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class NetworkManagerGuard : MonoBehaviour
{
    private static NetworkManagerGuard instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning(
                "NetworkManager already exists. " +
                "Destroying the duplicate."
            );

            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        Debug.Log("NetworkManager unique initialized.");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
