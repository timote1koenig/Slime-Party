using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MiniGameLauncher : MonoBehaviour
{
    public static MiniGameLauncher Instance { get; private set; }
    [SerializeField] private List<string> miniGameScenes;
    private int currentGameIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
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

        currentGameIndex = (currentGameIndex + 1) % miniGameScenes.Count;
        string nextGameScene = miniGameScenes[currentGameIndex];

        NetworkManager.Singleton.SceneManager.LoadScene(
            nextGameScene,
            LoadSceneMode.Single
        );
    }
}
