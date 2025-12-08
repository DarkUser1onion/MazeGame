using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MazeGame.Controls;
using MazeGame.Services;

namespace MazeGame;

public partial class MainWindow : Window
{
    private readonly MazeGenerator _generator = new();
    private (int X, int Y) _playerPosition;
    private bool _finish;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        GenerateMaze();
    }

    private void OnGenerateMaze(object? sender, RoutedEventArgs e)
    {
        GenerateMaze();
    }

    private void GenerateMaze()
    {
        _finish = false;
        
        if (MazeCanvas is not MazeView view || WidthInput is null || HeightInput is null)
            return;

        var width = (int)Math.Clamp(WidthInput.Value ?? 10, WidthInput.Minimum, WidthInput.Maximum);
        var height = (int)Math.Clamp(HeightInput.Value ?? 10, HeightInput.Minimum, HeightInput.Maximum);

        int? seed = null;
        if (!string.IsNullOrWhiteSpace(SeedInput?.Text) && int.TryParse(SeedInput.Text, out var parsedSeed))
            seed = parsedSeed;
        

        view.Maze = _generator.Generate(width, height, seed);
        _playerPosition = (0, 0);
        view.PlayerPosition = _playerPosition;
        UpdateStatusText();
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (TryHandleMovement(e.Key))
            e.Handled = true;
    }

    private bool TryHandleMovement(Key key)
    {
        (int dx, int dy)? direction = key switch
        {
            Key.Left or Key.A => (-1, 0),
            Key.Right or Key.D => (1, 0),
            Key.Up or Key.W => (0, -1),
            Key.Down or Key.S => (0, 1),
            _ => null
        };

        if(_finish)
            return false;
        
        if (direction is null || MazeCanvas?.Maze is not { } maze)
            return false;

        var (dx, dy) = direction.Value;
        if (!maze.CanMove(_playerPosition.X, _playerPosition.Y, dx, dy))
            return false;

        _playerPosition = (_playerPosition.X + dx, _playerPosition.Y + dy);
        MazeCanvas.PlayerPosition = _playerPosition;
        UpdateStatusText();
        return true;
    }

    private void UpdateStatusText()
    {
        if (StatusText is null)
            return;

        if (MazeCanvas?.Maze is not { } maze)
        {
            StatusText.Text = "Сначала сгенерируйте лабиринт";
            return;
        }

        if (_playerPosition.X == maze.Width - 1 && _playerPosition.Y == maze.Height - 1)
        {
            StatusText.Text = "Финиш! Нажмите «Сгенерировать», чтобы сыграть ещё.";
            
            _finish = true;
        }
        else
        {
            StatusText.Text = "Управление стрелками или WASD.";
        }
    }
}