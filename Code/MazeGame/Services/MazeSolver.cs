using System;
using System.Collections.Generic;
using System.Linq;
using MazeGame.Models;

namespace MazeGame.Services;

public class MazeSolver
{
    /// <summary>
    /// Находит кратчайший путь от старта до цели, используя алгоритм поиска в ширину (BFS)
    /// </summary>
    public List<(int X, int Y)> FindPath(Maze maze, int startX, int startY, int targetX, int targetY)
    {
        if (startX == targetX && startY == targetY)
            return new List<(int X, int Y)> { (startX, startY) };

        // Используем BFS для поиска кратчайшего пути
        return FindPathBFS(maze, startX, startY, targetX, targetY);
    }

    /// <summary>
    /// Поиск кратчайшего пути с помощью алгоритма поиска в ширину (BFS)
    /// </summary>
    private List<(int X, int Y)> FindPathBFS(Maze maze, int startX, int startY, int targetX, int targetY)
    {
        var queue = new Queue<(int X, int Y)>();
        var visited = new HashSet<(int X, int Y)>();
        var parent = new Dictionary<(int X, int Y), (int X, int Y)?>();

        queue.Enqueue((startX, startY));
        visited.Add((startX, startY));
        parent[(startX, startY)] = null;

        var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var (x, y) = current;

            // Если достигли цели, восстанавливаем путь
            if (x == targetX && y == targetY)
            {
                return ReconstructPath(parent, (startX, startY), (targetX, targetY));
            }

            // Проверяем всех соседей
            foreach (var (dx, dy) in directions)
            {
                if (maze.CanMove(x, y, dx, dy))
                {
                    var nextX = x + dx;
                    var nextY = y + dy;
                    var next = (nextX, nextY);

                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        parent[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }
        }

        // Путь не найден - возвращаем путь только до старта
        return new List<(int X, int Y)> { (startX, startY) };
    }

    /// <summary>
    /// Восстанавливает путь от старта до цели по словарю parent
    /// </summary>
    private List<(int X, int Y)> ReconstructPath(Dictionary<(int X, int Y), (int X, int Y)?> parent, 
        (int X, int Y) start, (int X, int Y) target)
    {
        var path = new List<(int X, int Y)>();
        var current = target;

        // Восстанавливаем путь от цели к старту
        while (current != start)
        {
            path.Add(current);
            if (parent.TryGetValue(current, out var prev) && prev.HasValue)
            {
                current = prev.Value;
            }
            else
            {
                // Если не можем восстановить путь, возвращаем только старт
                return new List<(int X, int Y)> { start };
            }
        }

        path.Add(start);
        path.Reverse(); // Разворачиваем, чтобы получить путь от старта к цели
        return path;
    }
}



