using System;
using System.Collections.Generic;
using System.Linq;
using MazeGame.Models;

namespace MazeGame.Services;

public class MazeSolver
{
    /// <summary>
    /// Находит путь от старта до цели, используя алгоритм следования по стене (wall-following)
    /// с последующей обрезкой петель для получения кратчайшего пути
    /// </summary>
    public List<(int X, int Y)> FindPath(Maze maze, int startX, int startY, int targetX, int targetY)
    {
        if (startX == targetX && startY == targetY)
            return new List<(int X, int Y)> { (startX, startY) };

        // Алгоритм следования по правой стене
        var path = FollowWall(maze, startX, startY, targetX, targetY, followRight: true);
        
        // Если не нашли путь, пробуем следовать по левой стене
        if (!path.Contains((targetX, targetY)))
        {
            path = FollowWall(maze, startX, startY, targetX, targetY, followRight: false);
        }

        // Обрезаем петли для получения кратчайшего пути
        return RemoveLoops(path);
    }

    private List<(int X, int Y)> FollowWall(Maze maze, int startX, int startY, int targetX, int targetY, bool followRight)
    {
        var path = new List<(int X, int Y)> { (startX, startY) };
        var visited = new HashSet<(int X, int Y)>();
        var currentX = startX;
        var currentY = startY;
        var direction = (dx: 1, dy: 0); // Начинаем с движения вправо

        var maxSteps = maze.Width * maze.Height * 2; // Защита от бесконечного цикла
        var steps = 0;

        while (steps < maxSteps)
        {
            if (currentX == targetX && currentY == targetY)
                break;

            visited.Add((currentX, currentY));

            // Определяем приоритетные направления для следования по стене
            var directions = GetWallFollowingDirections(direction, followRight);
            
            bool moved = false;
            foreach (var (dx, dy) in directions)
            {
                if (maze.CanMove(currentX, currentY, dx, dy))
                {
                    var nextX = currentX + dx;
                    var nextY = currentY + dy;
                    
                    // Избегаем зацикливания (но разрешаем возврат если долго не находим цель)
                    if (visited.Contains((nextX, nextY)) && path.Count > 10)
                    {
                        // Разрешаем возврат только если мы далеко от цели
                        var distToTarget = Math.Abs(nextX - targetX) + Math.Abs(nextY - targetY);
                        if (distToTarget < 5)
                            continue;
                    }

                    currentX = nextX;
                    currentY = nextY;
                    direction = (dx, dy);
                    path.Add((currentX, currentY));
                    moved = true;
                    break;
                }
            }

            if (!moved)
                break; // Застряли

            steps++;
        }

        return path;
    }

    private List<(int dx, int dy)> GetWallFollowingDirections((int dx, int dy) currentDir, bool followRight)
    {
        // Приоритеты для следования по правой/левой стене
        // Всегда пытаемся повернуть в нужную сторону, затем идти прямо, затем в другую сторону, затем назад
        
        var right = followRight ? 1 : -1;
        var left = -right;

        return currentDir switch
        {
            (1, 0) => new List<(int, int)> { (0, right), (1, 0), (0, left), (-1, 0) }, // Вправо: поворот вниз/вверх, прямо, назад
            (-1, 0) => new List<(int, int)> { (0, left), (-1, 0), (0, right), (1, 0) }, // Влево
            (0, 1) => new List<(int, int)> { (left, 0), (0, 1), (right, 0), (0, -1) }, // Вниз
            (0, -1) => new List<(int, int)> { (right, 0), (0, -1), (left, 0), (0, 1) }, // Вверх
            _ => new List<(int, int)> { (1, 0), (0, 1), (-1, 0), (0, -1) }
        };
    }

    private List<(int X, int Y)> RemoveLoops(List<(int X, int Y)> path)
    {
        if (path.Count <= 2)
            return path;

        var result = new List<(int X, int Y)> { path[0] };
        var lastIndex = new Dictionary<(int X, int Y), int>();

        for (var i = 1; i < path.Count; i++)
        {
            var current = path[i];
            
            if (lastIndex.TryGetValue(current, out var lastPos))
            {
                // Нашли петлю - удаляем все элементы между lastPos и текущей позицией
                result.RemoveRange(lastPos, result.Count - lastPos);
            }

            result.Add(current);
            lastIndex[current] = result.Count - 1;
        }

        return result;
    }
}

