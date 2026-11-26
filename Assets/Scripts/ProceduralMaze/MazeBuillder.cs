using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeBuilder : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private int width = 30;
    [SerializeField] private int height = 30;
    [SerializeField] private int seed = 12345;

    [SerializeField, Range(0f, 1f)]
    private float braidChance = 0.65f;

    [SerializeField, Range(0f, 1f)]
    private float extraLoopChance = 0.08f;

    [Header("Geometry")]
    [SerializeField] private float cellSize = 4f;
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float wallThickness = 0.2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;

    [Header("Camera Animation")]
    [SerializeField] private MazeCameraAnimator cameraAnimator;

    [Header("Player Spawn")]
    [SerializeField] List<GameObject>playersPrefab;

    private static readonly WaitForSeconds _mazeBuilderWaitForSeconds = new(0.005f);

    private Transform generatedRoot;
    private List<Vector2Int> spawnCells;

    // for GUI debugging
    public int Width => width;
    public int Height => height;
    public int Seed => seed;

    public float BraidChance => braidChance;
    public float ExtraLoopChance => extraLoopChance;

    public float CellSize => cellSize;
    public float WallHeight => wallHeight;

    private void Start()
    {
        if (generateOnStart)
            Generate(width, height, seed, braidChance, extraLoopChance);
    }

    public void Generate(int width, int height, int seed, float braidChance, float extraLoopChance)
    {
        if (floorPrefab == null || wallPrefab == null)
        {
            Debug.LogError("MazeBuilder: Missing prefabs.");
            return;
        }

        ClearPreviousMaze();

        MazeData maze = MazeGenerator.Generate(
            width,
            height,
            seed,
            braidChance,
            extraLoopChance
        );

        generatedRoot = new GameObject("GeneratedMaze").transform;
        generatedRoot.SetParent(transform, false);

        Vector3 center = new((width * cellSize) / 2f - (cellSize / 2f), 0f, (height * cellSize) / 2f - (cellSize / 2f));

        spawnCells = MazeGenerator.GenerateSpawns(maze, playersPrefab.Count, seed, 12, 2);

        if (cameraAnimator != null)
            cameraAnimator.BeginOrbit(center);

        StartCoroutine(BuildMaze(maze, width, height, center));
    }

    private IEnumerator BuildMaze(MazeData maze, int width, int height, Vector3 center)
    {
        CreateFloor(center, width, height);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                yield return _mazeBuilderWaitForSeconds;
                Vector3 cellPosition = transform.position + new Vector3(x * cellSize, 0f, z * cellSize);

                MazeWall walls = maze.GetCell(x, z).Walls;

                if (z == height - 1 || (walls & MazeWall.North) != 0)
                    CreateWall(cellPosition + Vector3.forward * (cellSize / 2f), Quaternion.identity);
                if (x == width - 1 || (walls & MazeWall.East) != 0)
                    CreateWall(cellPosition + Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f));
                if (z == 0)
                    CreateWall(cellPosition - Vector3.forward * (cellSize / 2f), Quaternion.identity);
                if (x == 0)
                    CreateWall(cellPosition - Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f));
            }
        }

        List<GameObject> players = new();

        for (int i = 0; i < playersPrefab.Count; i++)
            players.Add(Instantiate(playersPrefab[i], new Vector3(spawnCells[i].x * cellSize, 0f, spawnCells[i].y * cellSize), Quaternion.identity, generatedRoot));

        if (cameraAnimator != null)
            yield return StartCoroutine(cameraAnimator.FinishOrbit(players[0].transform, 2f));
    }

    private void CreateFloor(Vector3 position, int width, int height)
    {
        GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, generatedRoot);

        floor.transform.localScale = new Vector3(width * 0.4f, 0f, height * 0.4f);
    }

    private void CreateWall(Vector3 position, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, position + Vector3.up * (wallHeight / 2f), rotation, generatedRoot);

        wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
    }

    private void ClearPreviousMaze()
    {
        Transform previous = transform.Find("GeneratedMaze");

        if (previous == null)
            return;

        if (Application.isPlaying)
            Destroy(previous.gameObject);
        else
            DestroyImmediate(previous.gameObject);
    }
}
