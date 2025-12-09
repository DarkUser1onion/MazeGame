using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MazeGame.Models;

namespace MazeGame.Controls;

public class MazeView : Control
{
    public static readonly StyledProperty<Maze?> MazeProperty =
        AvaloniaProperty.Register<MazeView, Maze?>(nameof(Maze));

    public static readonly StyledProperty<double> WallThicknessProperty =
        AvaloniaProperty.Register<MazeView, double>(nameof(WallThickness), 2);

    public static readonly StyledProperty<IBrush?> PathBrushProperty =
        AvaloniaProperty.Register<MazeView, IBrush?>(nameof(PathBrush), Brushes.White);

    public static readonly StyledProperty<IBrush?> StartBrushProperty =
        AvaloniaProperty.Register<MazeView, IBrush?>(nameof(StartBrush), Brushes.LightGreen);

    public static readonly StyledProperty<IBrush?> FinishBrushProperty =
        AvaloniaProperty.Register<MazeView, IBrush?>(nameof(FinishBrush), Brushes.IndianRed);

    public static readonly StyledProperty<(int X, int Y)> PlayerPositionProperty =
        AvaloniaProperty.Register<MazeView, (int X, int Y)>(nameof(PlayerPosition), (0, 0));

    public static readonly StyledProperty<List<(int X, int Y)>> PathProperty =
        AvaloniaProperty.Register<MazeView, List<(int X, int Y)>>(nameof(Path), new List<(int X, int Y)>());

    public static readonly StyledProperty<IBrush?> PathLineBrushProperty =
        AvaloniaProperty.Register<MazeView, IBrush?>(nameof(PathLineBrush), Brushes.Yellow);

    static MazeView()
    {
        AffectsRender<MazeView>(
            MazeProperty,
            WallThicknessProperty,
            PathBrushProperty,
            StartBrushProperty,
            FinishBrushProperty,
            PlayerPositionProperty,
            PathProperty,
            PathLineBrushProperty);
    }

    public Maze? Maze
    {
        get => GetValue(MazeProperty);
        set => SetValue(MazeProperty, value);
    }

    public double WallThickness
    {
        get => GetValue(WallThicknessProperty);
        set => SetValue(WallThicknessProperty, value);
    }

    public IBrush? PathBrush
    {
        get => GetValue(PathBrushProperty);
        set => SetValue(PathBrushProperty, value);
    }

    public IBrush? StartBrush
    {
        get => GetValue(StartBrushProperty);
        set => SetValue(StartBrushProperty, value);
    }

    public IBrush? FinishBrush
    {
        get => GetValue(FinishBrushProperty);
        set => SetValue(FinishBrushProperty, value);
    }

    public (int X, int Y) PlayerPosition
    {
        get => GetValue(PlayerPositionProperty);
        set => SetValue(PlayerPositionProperty, value);
    }

    public List<(int X, int Y)> Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public IBrush? PathLineBrush
    {
        get => GetValue(PathLineBrushProperty);
        set => SetValue(PathLineBrushProperty, value);
    }

    public (int X, int Y)? GetCellFromPoint(Point point)
    {
        if (Maze is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return null;

        var cellSize = CalculateCellSize(Maze);
        var offsetX = (Bounds.Width - Maze.Width * cellSize) / 2;
        var offsetY = (Bounds.Height - Maze.Height * cellSize) / 2;

        var relativeX = point.X - offsetX;
        var relativeY = point.Y - offsetY;

        if (relativeX < 0 || relativeY < 0)
            return null;

        var cellX = (int)(relativeX / cellSize);
        var cellY = (int)(relativeY / cellSize);

        if (cellX >= 0 && cellX < Maze.Width && cellY >= 0 && cellY < Maze.Height)
            return (cellX, cellY);

        return null;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Maze is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var maze = Maze;
        var pen = new Pen(PathBrush ?? Brushes.White, WallThickness, lineCap: PenLineCap.Round);

        var cellSize = CalculateCellSize(maze);
        var offsetX = (Bounds.Width - maze.Width * cellSize) / 2;
        var offsetY = (Bounds.Height - maze.Height * cellSize) / 2;
        var drawingRect = new Rect(offsetX, offsetY, maze.Width * cellSize, maze.Height * cellSize);

        context.FillRectangle(Brushes.Black, drawingRect);

        DrawStartAndFinish(context, cellSize, offsetX, offsetY, maze);
        DrawPath(context, cellSize, offsetX, offsetY, maze);
        DrawPlayer(context, cellSize, offsetX, offsetY, maze);
        DrawHorizontalWalls(context, cellSize, offsetX, offsetY, maze, pen);
        DrawVerticalWalls(context, cellSize, offsetX, offsetY, maze, pen);
    }

    private double CalculateCellSize(Maze maze)
        => Math.Min(Bounds.Width / maze.Width, Bounds.Height / maze.Height);

    private void DrawHorizontalWalls(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze, Pen pen)
    {
        for (var i = 0; i <= maze.Height; i++)
            for (var j = 0; j < maze.Width; j++)
            {
                if (!maze.HorizontalWalls[i, j])
                    continue;

                var start = new Point(offsetX + j * cell, offsetY + i * cell);
                var end = new Point(start.X + cell, start.Y);
                context.DrawLine(pen, start, end);
            }
    }

    private void DrawVerticalWalls(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze, Pen pen)
    {
        for (var i = 0; i < maze.Height; i++)
            for (var j = 0; j <= maze.Width; j++)
            {
                if (!maze.VerticalWalls[i, j])
                    continue;

                var start = new Point(offsetX + j * cell, offsetY + i * cell);
                var end = new Point(start.X, start.Y + cell);
                context.DrawLine(pen, start, end);
            }
    }

    private void DrawStartAndFinish(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze)
    {
        var startRect = new Rect(offsetX + 0.15 * cell, offsetY + 0.15 * cell, cell * 0.7, cell * 0.7);
        var finishRect = new Rect(offsetX + (maze.Width - 1 + 0.15) * cell, offsetY + (maze.Height - 1 + 0.15) * cell, cell * 0.7, cell * 0.7);

        if (StartBrush is not null)
            context.FillRectangle(StartBrush, startRect);

        if (FinishBrush is not null)
            context.FillRectangle(FinishBrush, finishRect);
    }

    private void DrawPath(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze)
    {
        if (Path is null || Path.Count < 2 || PathLineBrush is null)
            return;

        var pen = new Pen(PathLineBrush, cell * 0.15, lineCap: PenLineCap.Round);
        var points = new List<Point>();

        foreach (var (x, y) in Path)
        {
            if (x < 0 || y < 0 || x >= maze.Width || y >= maze.Height)
                continue;

            var center = new Point(
                offsetX + (x + 0.5) * cell,
                offsetY + (y + 0.5) * cell);
            points.Add(center);
        }

        if (points.Count < 2)
            return;

        // Рисуем линии между точками пути
        for (var i = 0; i < points.Count - 1; i++)
        {
            context.DrawLine(pen, points[i], points[i + 1]);
        }

        // Рисуем точки на пути
        IBrush? pointBrush = null;
        if (PathLineBrush is ISolidColorBrush solidBrush)
        {
            var color = solidBrush.Color;
            var semiTransparentColor = new Color(153, color.R, color.G, color.B); // 60% opacity
            pointBrush = new SolidColorBrush(semiTransparentColor);
        }
        else
        {
            pointBrush = PathLineBrush;
        }
        
        var pointSize = cell * 0.15;
        foreach (var point in points)
        {
            if (pointBrush is not null)
            {
                var rect = new Rect(point.X - pointSize / 2, point.Y - pointSize / 2, pointSize, pointSize);
                context.FillRectangle(pointBrush, rect);
            }
        }
    }

    private void DrawPlayer(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze)
    {
        var (px, py) = PlayerPosition;
        if (px < 0 || py < 0 || px >= maze.Width || py >= maze.Height)
            return;

        var center = new Point(
            offsetX + (px + 0.5) * cell,
            offsetY + (py + 0.5) * cell);

        var size = cell * 0.4;
        var rect = new Rect(center.X - size / 2, center.Y - size / 2, size, size);
        context.FillRectangle(Brushes.DeepSkyBlue, rect);
    }
}

