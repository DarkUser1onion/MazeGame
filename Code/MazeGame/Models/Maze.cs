using System;

namespace MazeGame.Models;

public class Maze
{
    public int Width { get; }
    public int Height { get; }
    public bool[,] HorizontalWalls { get; }
    public bool[,] VerticalWalls { get; }

    public Maze(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
        HorizontalWalls = new bool[height + 1, width];
        VerticalWalls = new bool[height, width + 1];

        for (var i = 0; i <= height; i++)
            for (var j = 0; j < width; j++)
                HorizontalWalls[i, j] = true;

        for (var i = 0; i < height; i++)
            for (var j = 0; j <= width; j++)
                VerticalWalls[i, j] = true;
    }


    public void RemoveWallBetween(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2 && y1 == y2)
            return;

        var distance = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        if (distance != 1)
            throw new ArgumentException("Cells must be adjacent to remove a wall.");

        if (x1 == x2)
        {
            var row = Math.Max(y1, y2);
            HorizontalWalls[row, x1] = false;
        }
        else
        {
            var column = Math.Max(x1, x2);
            VerticalWalls[y1, column] = false;
        }
    }

    public bool CanMove(int x, int y, int dx, int dy)
    {
        if (dx == 0 && dy == 0)
            return false;

        var targetX = x + dx;
        var targetY = y + dy;

        if (targetX < 0 || targetY < 0 || targetX >= Width || targetY >= Height)
            return false;

        if (dx == -1 && VerticalWalls[y, x])
            return false;

        if (dx == 1 && VerticalWalls[y, x + 1])
            return false;

        if (dy == -1 && HorizontalWalls[y, x])
            return false;

        if (dy == 1 && HorizontalWalls[y + 1, x])
            return false;

        return true;
    }
}

