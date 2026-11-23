using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public SlimeSlingshot slime;
    public TMP_Text statusText;
    [Range(0f, 1f)] public float clearRatio = 1f;

    [Header("Score")]
    public int bonusPerSlime = 500;
    public int maxTimeBonus = 1000;
    public float timeBonusLimit = 60f; // 이 시간 안에 깨야 시간 보너스

    enum GameState { Playing, Clear, Fail }
    GameState state = GameState.Playing;

    KnockableBlock[] blocks;
    float elapsed;
    public int Score { get; private set; }

    void Start()
    {
        blocks = FindObjectsByType<KnockableBlock>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (state != GameState.Playing)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        elapsed += Time.deltaTime;

        int knocked = 0;
        int blockScore = 0;
        foreach (var b in blocks)
        {
            if (b.IsKnocked)
            {
                knocked++;
                blockScore += b.points; // 블록마다 다른 점수
            }
        }
        Score = blockScore;

        if (knocked >= Mathf.CeilToInt(blocks.Length * clearRatio))
        {
            OnClear(blockScore);
            return;
        }

        if (slime.CurrentState == SlimeSlingshot.State.Done)
        {
            OnFail();
            return;
        }

        statusText.text = $"Score {Score}   Slimes {slime.ShotsLeft}   Time {elapsed:0.0}s";
    }

    void OnClear(int blockScore)
    {
        state = GameState.Clear;
        slime.CanShoot = false;

        int slimeBonus = slime.ShotsLeft * bonusPerSlime;
        int timeBonus = Mathf.RoundToInt(maxTimeBonus * Mathf.Clamp01(1f - elapsed / timeBonusLimit));
        Score = blockScore + slimeBonus + timeBonus;

        statusText.text =
            $"CLEAR!\n<size=45%>Blocks {blockScore} + Slimes {slimeBonus} + Time {timeBonus}\n" +
            $"Score {Score}  ({elapsed:0.0}s)\nPress Enter to restart</size>";
    }

    void OnFail()
    {
        state = GameState.Fail;
        statusText.text = $"FAILED\n<size=45%>Score {Score}\nPress Enter to restart</size>";
    }
}