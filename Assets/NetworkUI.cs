using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkUI : MonoBehaviour
{
    [SerializeField]
    private RelayConnectionManager relayManager;

    private void Awake()
    {
        if (relayManager == null)
            relayManager = GetComponent<RelayConnectionManager>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            relayManager.StartHostWithRelay();
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            relayManager.StartClientWithRelay();
        }

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            LoadMiniGame();
        }

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            LoadSecondMiniGame();
        }
    }

    private void LoadMiniGame()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "SampleScene",
            LoadSceneMode.Single
        );
    }

    private void LoadSecondMiniGame()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "SecondMiniGameScene",
            LoadSceneMode.Single
        );
    }
}
