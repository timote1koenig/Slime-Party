using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLauncher : MonoBehaviour
{
    public void LoadNextGame()
    {
        LoadMiniGame();
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
}
