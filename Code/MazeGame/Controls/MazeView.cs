using System;
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

    static MazeView()
    {
        AffectsRender<MazeView>(
            MazeProperty,
            WallThicknessProperty,
            PathBrushProperty,
            StartBrushProperty,
            FinishBrushProperty,
            PlayerPositionProperty);
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

    private void DrawPlayer(DrawingContext context, double cell, double offsetX, double offsetY, Maze maze)
    {
        var (px, py) = PlayerPosition;
        if (px < 0 || py < 0 || px >= maze.Width || py >= maze.Height)
            return;

        var center = new Point(
            offsetX + (px + 0.5) * cell,
            offsetY + (py + 0.5) * cell);

        var radius = cell * 0.25;
        var rect = new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        context.FillRectangle(Brushes.BlueViolet, rect);
    }
}

