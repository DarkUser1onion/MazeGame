using System;
using System.Collections.Generic;
using MazeGame.Models;

namespace MazeGame.Services;

public class MazeGenerator
{
    public Maze Generate(int width, int height, int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        var maze = new Maze(width, height);
        var visited = new bool[height, width];
        var stack = new Stack<(int X, int Y)>();

        stack.Push((0, 0));
        visited[0, 0] = true;

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var neighbors = GetUnvisitedNeighbors(current.X, current.Y, width, height, visited);

            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            var next = neighbors[random.Next(neighbors.Count)];
            maze.RemoveWallBetween(current.X, current.Y, next.X, next.Y);
            visited[next.Y, next.X] = true;
            stack.Push(next);
        }

        return maze;
    }

    private static List<(int X, int Y)> GetUnvisitedNeighbors(int x, int y, int width, int height, bool[,] visited)
    {
        var neighbors = new List<(int, int)>(4);

        void TryAdd(int nx, int ny)
        {
            if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[ny, nx])
                neighbors.Add((nx, ny));
        }

        TryAdd(x - 1, y);
        TryAdd(x + 1, y);
        TryAdd(x, y - 1);
        TryAdd(x, y + 1);

        return neighbors;
    }
}




