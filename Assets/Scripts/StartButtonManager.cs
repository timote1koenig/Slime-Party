using UnityEngine;
using UnityEngine.UI;

public class StartButtonManager : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(LoadNextGame);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(LoadNextGame);
    }

    private void LoadNextGame()
    {
        if (MiniGameLauncher.Instance != null)
        {
            MiniGameLauncher.Instance.LoadNextGame();
        }
    }
}