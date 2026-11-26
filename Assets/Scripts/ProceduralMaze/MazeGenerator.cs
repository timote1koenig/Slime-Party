using UnityEngine;
using System.Collections.Generic;

public static class MazeGenerator
{
    private static readonly MazeWall[] Directions =
    {
        MazeWall.North,
        MazeWall.East,
        MazeWall.South,
        MazeWall.West
    };

    public static MazeData Generate(int width, int height, int seed, float braidChance, float extraLoopChance)
    {
        var maze = new MazeData(width, height);

        System.Random random = new(seed);

        braidChance = Mathf.Clamp01(braidChance);
        extraLoopChance = Mathf.Clamp01(extraLoopChance);

        GenerateDFS(maze, random);
        RemoveDeadEnds(maze, random, braidChance);
        AddExtraLoops(maze, random, extraLoopChance);

        return maze;
    }

    private static void GenerateDFS(MazeData maze, System.Random random)
    {
        List<Vector2Int> stack = new();
        Vector2Int start = new(random.Next(maze.Width), random.Next(maze.Height));

        maze.GetCell(start.x, start.y).Visited = true;
        stack.Add(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack[^1];
            List<MazeWall> available = new();

            foreach (MazeWall direction in Directions)
            {
                Vector2Int next = maze.GetNeighbor(current, direction);

                if (!maze.IsInside(next.x, next.y))
                    continue;

                if (!maze.GetCell(next.x, next.y).Visited)
                    available.Add(direction);
            }

            if (available.Count == 0)
            {
                stack.RemoveAt(stack.Count - 1);
                continue;
            }

            MazeWall chosen = available[random.Next(available.Count)];

            Vector2Int neighbor = maze.GetNeighbor(current, chosen);

            maze.RemoveWall(current.x, current.y, chosen);
            maze.GetCell(neighbor.x, neighbor.y).Visited = true;
            stack.Add(neighbor);
        }
    }

    private static void RemoveDeadEnds(MazeData maze, System.Random random, float chance)
    {
        List<Vector2Int> deadEnds = new();

        for (int x = 0; x < maze.Width; x++)
            for (int z = 0; z < maze.Height; z++)
                if (CountWalls(maze.GetCell(x, z)) == 3)
                    deadEnds.Add(new Vector2Int(x, z));

        foreach (Vector2Int position in deadEnds)
        {
            if (random.NextDouble() > chance)
                continue;

            List<MazeWall> possibleWalls = new();

            foreach (MazeWall direction in Directions)
            {
                Vector2Int neighbor = maze.GetNeighbor(position, direction);

                if (!maze.IsInside(neighbor.x, neighbor.y))
                    continue;

                if ((maze.GetCell(position.x, position.y).Walls & direction) != 0)
                    possibleWalls.Add(direction);
            }

            if (possibleWalls.Count == 0)
                continue;

            MazeWall chosen = possibleWalls[random.Next(possibleWalls.Count)];

            maze.RemoveWall(position.x, position.y, chosen);
        }
    }

    private static void AddExtraLoops(MazeData maze, System.Random random, float chance)
    {
        for (int x = 0; x < maze.Width; x++)
        {
            for (int z = 0; z < maze.Height; z++)
            {
                if (x < maze.Width - 1 && HasWall(maze, x, z, MazeWall.East) && random.NextDouble() < chance)
                    maze.RemoveWall(x, z, MazeWall.East);

                if (z < maze.Height - 1 && HasWall(maze, x, z, MazeWall.North) && random.NextDouble() < chance)
                    maze.RemoveWall(x, z, MazeWall.North);
            }
        }
    }

    private static bool HasWall(MazeData maze, int x, int z, MazeWall wall)
    {
        return (maze.GetCell(x, z).Walls & wall) != 0;
    }

    private static int CountWalls(MazeCell cell)
    {
        int count = 0;

        foreach (MazeWall direction in Directions)
            if ((cell.Walls & direction) != 0)
                count++;
        return count;
    }

    public static List<Vector2Int> GenerateSpawns(MazeData maze, int spawnCount, int seed, int minSpawnDistance = 12, int minOpenings = 2)
    {
        List<Vector2Int> selected = new();
        List<Vector2Int> candidates = new();

        if (maze == null || spawnCount <= 0)
            return selected;

        System.Random random = new(seed);

        for (int x = 0; x < maze.Width; x++)
            for (int z = 0; z < maze.Height; z++)
                if (CountOpenings(maze, x, z) >= minOpenings)
                    candidates.Add(new Vector2Int(x, z));

        if (candidates.Count < spawnCount)
        {
            candidates.Clear();

            for (int x = 0; x < maze.Width; x++)
                for (int z = 0; z < maze.Height; z++)
                    if (CountOpenings(maze, x, z) >= 1)
                        candidates.Add(new Vector2Int(x, z));
        }

        if (candidates.Count < spawnCount)
        {
            Debug.LogError("Not enough valid spawn positions!");
            return selected;
        }

        Vector2Int first = candidates[random.Next(candidates.Count)];

        selected.Add(first);

        int[,] minimumDistances = maze.GetDistanceMap(first.x, first.y);

        while (selected.Count < spawnCount)
        {
            int bestDistance = -1;

            List<Vector2Int> bestCandidates = new();

            foreach (Vector2Int candidate in candidates)
            {
                if (selected.Contains(candidate))
                    continue;

                int distance = minimumDistances[candidate.x, candidate.y];

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestCandidates.Clear();
                    bestCandidates.Add(candidate);
                }
                else if (distance == bestDistance)
                    bestCandidates.Add(candidate);
            }

            if (bestCandidates.Count == 0)
                break;

            Vector2Int bestCandidate = bestCandidates[random.Next(bestCandidates.Count)];

            if (bestDistance < minSpawnDistance)
                Debug.LogWarning($"Spawn distance below target: {bestDistance} " + $"(target: {minSpawnDistance})");

            selected.Add(bestCandidate);

            int[,] newDistances = maze.GetDistanceMap(bestCandidate.x, bestCandidate.y);

            for (int x = 0; x < maze.Width; x++)
            {
                for (int z = 0; z < maze.Height; z++)
                {
                    int newDistance = newDistances[x, z];

                    if (newDistance >= 0 && (minimumDistances[x, z] < 0 || newDistance < minimumDistances[x, z]))
                        minimumDistances[x, z] = newDistance;
                }
            }
        }

        if (selected.Count < spawnCount)
            Debug.LogWarning($"Only {selected.Count}/{spawnCount} spawns found. " + "Try reducing minSpawnDistance.");
        return selected;
    }

    private static int CountOpenings(MazeData maze, int x, int z)
    {
        MazeCell cell = maze.GetCell(x, z);
        int openings = 0;

        foreach (MazeWall direction in new MazeWall[] { MazeWall.North, MazeWall.East, MazeWall.South, MazeWall.West })
        {
            if ((cell.Walls & direction) == 0)
                continue;

            Vector2Int next = maze.GetNeighbor(new Vector2Int(x, z), direction);

            if (maze.IsInside(next.x, next.y))
                continue;
        }

        foreach (MazeWall direction in new MazeWall[] { MazeWall.North, MazeWall.East, MazeWall.South, MazeWall.West })
        {
            Vector2Int next = maze.GetNeighbor(new Vector2Int(x, z), direction);

            if (!maze.IsInside(next.x, next.y))
                continue;

            if ((cell.Walls & direction) == 0)
                openings++;
        }
        return openings;
    }
}