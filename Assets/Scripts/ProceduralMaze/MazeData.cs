using System.Collections.Generic;
using UnityEngine;

public enum MazeWall
{
    None = 0,
    North = 1,
    South = 2,
    East = 4,
    West = 8,
    All = North | South | East | West
}

public class MazeCell
{
    public bool Visited = false;
    public MazeWall Walls = MazeWall.All;
}

public class MazeData
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MazeCell[,] Cells { get; private set; }

    public MazeData(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new MazeCell[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Cells[x, y] = new MazeCell { Walls = MazeWall.All };
    }

    public MazeCell GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Cells[x, y];
    }

    public bool IsInside(int x, int z)
    {
        return x >= 0 && x < Width && z >= 0 && z < Height;
    }

    public void RemoveWall(int x, int y, MazeWall direction)
    {
        int neighborX = x;
        int neighborY = y;
        MazeCell cell = GetCell(x, y);
        MazeWall oppositeDirection = MazeWall.None;

        switch (direction)
        {
            case MazeWall.North:
                neighborY -= 1;
                oppositeDirection = MazeWall.South;
                break;
            case MazeWall.South:
                neighborY += 1;
                oppositeDirection = MazeWall.North;
                break;
            case MazeWall.East:
                neighborX += 1;
                oppositeDirection = MazeWall.West;
                break;
            case MazeWall.West:
                neighborX -= 1;
                oppositeDirection = MazeWall.East;
                break;
        }
        cell.Walls &= ~direction;

        MazeCell neighborCell = GetCell(neighborX, neighborY);

        if (neighborCell != null)
            neighborCell.Walls &= ~oppositeDirection;
    }



    public int[,] GetDistanceMap(int startX, int startZ)
    {
        int[,] distances = new int[Width, Height];

        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Height; z++)
                distances[x, z] = -1;

        if (!IsInside(startX, startZ))
            return distances;

        Queue<Vector2Int> queue = new();

        Vector2Int start = new(startX, startZ);

        distances[startX, startZ] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (MazeWall direction in new MazeWall[] { MazeWall.North, MazeWall.East, MazeWall.South, MazeWall.West })
            {
                if ((GetCell(current.x, current.y).Walls & direction) != 0)
                    continue;

                Vector2Int next = GetNeighbor(current, direction);

                if (!IsInside(next.x, next.y) || distances[next.x, next.y] != -1)
                    continue;

                distances[next.x, next.y] = distances[current.x, current.y] + 1;

                queue.Enqueue(next);
            }
        }
        return distances;
    }

    public Vector2Int GetNeighbor(Vector2Int position, MazeWall direction)
    {
        return direction switch
        {
            MazeWall.North => position + Vector2Int.up,
            MazeWall.East => position + Vector2Int.right,
            MazeWall.South => position + Vector2Int.down,
            MazeWall.West => position + Vector2Int.left,
            _ => position,
        };
    }
}
