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

        // Базистый DFS для лабиринта
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

        // усложнение
        AddDeadEnds(maze, random, width, height);

        return maze;
    }

    private static void AddDeadEnds(Maze maze, Random random, int width, int height)
    {
        // Подсчитываем количество открытых проходов для каждой клетки
        var openPassages = new int[height, width];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (maze.CanMove(x, y, -1, 0)) openPassages[y, x]++;
            if (maze.CanMove(x, y, 1, 0)) openPassages[y, x]++;
            if (maze.CanMove(x, y, 0, -1)) openPassages[y, x]++;
            if (maze.CanMove(x, y, 0, 1)) openPassages[y, x]++;
        }

        // Если 3+ открытых проходов, то тупик
        var cellsToModify = new List<(int X, int Y)>();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            // Пропускаем старт и финиш
            if ((x == 0 && y == 0) || (x == width - 1 && y == height - 1))
                continue;

            if (openPassages[y, x] >= 3)
                cellsToModify.Add((x, y));
        }

        
        var toClose = (int)(cellsToModify.Count * (0.2 + random.NextDouble() * 0.1));
        toClose = Math.Min(toClose, cellsToModify.Count);

        var attempts = 0;
        var maxAttempts = toClose * 3; // Ограничиваем попытки

        for (var i = 0; i < toClose && attempts < maxAttempts; attempts++)
        {
            if (cellsToModify.Count == 0)
                break;

            var idx = random.Next(cellsToModify.Count);
            var (x, y) = cellsToModify[idx];

            // Выбираем случайный проход для закрытия
            var directions = new List<(int dx, int dy)>();
            if (maze.CanMove(x, y, -1, 0)) directions.Add((-1, 0));
            if (maze.CanMove(x, y, 1, 0)) directions.Add((1, 0));
            if (maze.CanMove(x, y, 0, -1)) directions.Add((0, -1));
            if (maze.CanMove(x, y, 0, 1)) directions.Add((0, 1));

            if (directions.Count <= 1)
            {
                cellsToModify.RemoveAt(idx);
                continue; // Нельзя закрывать, оставляем как есть
            }

            var dir = directions[random.Next(directions.Count)];
            var (dx, dy) = dir;
            var nx = x + dx;
            var ny = y + dy;

            // Сохраняем состояние стены перед изменением
            bool wallWasOpen;
            if (dx == -1)
            {
                wallWasOpen = !maze.VerticalWalls[y, x];
                maze.VerticalWalls[y, x] = true;
            }
            else if (dx == 1)
            {
                wallWasOpen = !maze.VerticalWalls[y, x + 1];
                maze.VerticalWalls[y, x + 1] = true;
            }
            else if (dy == -1)
            {
                wallWasOpen = !maze.HorizontalWalls[y, x];
                maze.HorizontalWalls[y, x] = true;
            }
            else // dy == 1
            {
                wallWasOpen = !maze.HorizontalWalls[y + 1, x];
                maze.HorizontalWalls[y + 1, x] = true;
            }

            // Проверка связности
            if (!IsConnected(maze, width, height))
            {
                // Если неверно, то откат изменение
                if (dx == -1) maze.VerticalWalls[y, x] = !wallWasOpen;
                else if (dx == 1) maze.VerticalWalls[y, x + 1] = !wallWasOpen;
                else if (dy == -1) maze.HorizontalWalls[y, x] = !wallWasOpen;
                else maze.HorizontalWalls[y + 1, x] = !wallWasOpen;
                
                cellsToModify.RemoveAt(idx);
                continue;
            }

            // Успешно закрыли проход
            cellsToModify.RemoveAt(idx);
            i++;
        }
    }

    private static bool IsConnected(Maze maze, int width, int height)
    {
        var visited = new bool[height, width];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((0, 0));
        visited[0, 0] = true;
        var visitedCount = 1;

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dx, dy) in directions)
            {
                if (maze.CanMove(x, y, dx, dy))
                {
                    var nx = x + dx;
                    var ny = y + dy;
                    if (!visited[ny, nx])
                    {
                        visited[ny, nx] = true;
                        visitedCount++;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        return visitedCount == width * height;
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




