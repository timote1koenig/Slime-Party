using UnityEngine;

[RequireComponent(typeof(MazeBuilder))]
public class MazeDebugUI : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebug = true;

    private MazeBuilder mazeBuilder;

    private int width = 30;
    private int height = 30;
    private int seed = 12345;

    private float braidChance = 0.65f;
    private float extraLoopChance = 0.08f;

    private Vector2 scrollPosition;

    private void Start()
    {
        mazeBuilder = GetComponent<MazeBuilder>();
        
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        seed = mazeBuilder.Seed;
        width = mazeBuilder.Width;
        height = mazeBuilder.Height;
        braidChance = mazeBuilder.BraidChance;
        extraLoopChance = mazeBuilder.ExtraLoopChance;
    }

    private void OnGUI()
    {
        if (!showDebug || mazeBuilder == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20), GUI.skin.box);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("MAZE DEBUG", GUI.skin.box);

        GUILayout.Space(10);

        GUILayout.Label("Dimensions");

        width = IntField("Width", width);
        height = IntField("Height", height);

        GUILayout.Space(10);

        GUILayout.Label("Generation");

        seed = IntField("Seed", seed);

        braidChance = FloatSlider("Braid Chance", braidChance);

        extraLoopChance = FloatSlider("Extra Loop Chance", extraLoopChance);

        GUILayout.Space(15);

        if (GUILayout.Button("REGENERATE MAZE", GUILayout.Height(40)))
            mazeBuilder.Generate(width, height, seed, braidChance, extraLoopChance);
        if (GUILayout.Button("RANDOM SEED"))
            seed = Random.Range(0, int.MaxValue);

        GUILayout.Space(10);

        GUILayout.Label("Current Seed: " + seed);

        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private int IntField(string label, int value)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label, GUILayout.Width(120));

        string input = GUILayout.TextField(value.ToString());

        if (int.TryParse(input, out int result))
            value = result;

        GUILayout.EndHorizontal();

        return value;
    }

    private float FloatSlider(string label, float value)
    {
        GUILayout.Label($"{label}: {value:P0}");

        return GUILayout.HorizontalSlider(value, 0f, 1f);
    }
}
