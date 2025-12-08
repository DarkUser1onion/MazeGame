using System.Collections.Generic;
using MazeGame.Models;
using MazeGame.Services;
using Xunit;

namespace MazeGame.Tests;

public class MazeLogicTests
{
    private static readonly (int dx, int dy)[] Directions =
    [
        (-1, 0),
        (1, 0),
        (0, -1),
        (0, 1)
    ];

    [Fact]
    public void Generate_CreatesMazeReachableFromStart()
    {
        var generator = new MazeGenerator();
        var maze = generator.Generate(8, 6, seed: 42);

        var visited = new bool[maze.Height, maze.Width];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((0, 0));
        visited[0, 0] = true;
        var visitedCount = 0;

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            visitedCount++;

            foreach (var (dx, dy) in Directions)
            {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= maze.Width || ny >= maze.Height)
                    continue;

                if (visited[ny, nx])
                    continue;

                if (!maze.CanMove(x, y, dx, dy))
                    continue;

                visited[ny, nx] = true;
                queue.Enqueue((nx, ny));
            }
        }

        Assert.Equal(maze.Width * maze.Height, visitedCount);
    }

    [Fact]
    public void CanMove_HonorsWallsAndBounds()
    {
        var maze = new Maze(2, 2);

        Assert.False(maze.CanMove(0, 0, -1, 0));
        Assert.False(maze.CanMove(0, 0, 0, -1));

        maze.RemoveWallBetween(0, 0, 1, 0);
        Assert.True(maze.CanMove(0, 0, 1, 0));
        Assert.False(maze.CanMove(0, 0, -1, 0));

        maze.RemoveWallBetween(1, 0, 1, 1);
        Assert.True(maze.CanMove(1, 0, 0, 1));
        Assert.False(maze.CanMove(1, 0, 0, -1));
    }
}
