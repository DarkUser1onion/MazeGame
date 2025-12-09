using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MazeGame.Controls;
using MazeGame.Models;
using MazeGame.Services;

namespace MazeGame;

public partial class MainWindow : Window
{
    private readonly MazeGenerator _generator = new();
    private readonly MazeSolver _solver = new();
    private readonly List<LeaderboardEntry> _leaderboard = new();
    
    private (int X, int Y) _playerPosition;
    private bool _isFinished;
    private int _steps;
    private Stopwatch _gameTimer = new();
    private bool _isAiRunning;
    private (int X, int Y)? _targetPosition;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        
        if (MazeCanvas is not null)
        {
            MazeCanvas.PointerPressed += OnMazeCanvasPointerPressed;
            MazeCanvas.PointerMoved += OnMazeCanvasPointerMoved;
            MazeCanvas.PointerReleased += OnMazeCanvasPointerReleased;
        }
        
        GenerateMaze();
    }

    private void OnGenerateMaze(object? sender, RoutedEventArgs e)
    {
        GenerateMaze();
    }

    private void GenerateMaze()
    {
        _isFinished = false;
        _steps = 0;
        _gameTimer.Reset();
        _gameTimer.Start();
        _isAiRunning = false;
        _targetPosition = null;
        
        if (MazeCanvas is not MazeView view || WidthInput is null || HeightInput is null)
            return;

        var width = (int)Math.Clamp(WidthInput.Value ?? 10, WidthInput.Minimum, WidthInput.Maximum);
        var height = (int)Math.Clamp(HeightInput.Value ?? 10, HeightInput.Minimum, HeightInput.Maximum);

        int? seed = null;
        if (!string.IsNullOrWhiteSpace(SeedInput?.Text) && int.TryParse(SeedInput.Text, out var parsedSeed))
            seed = parsedSeed;

        view.Maze = _generator.Generate(width, height, seed);
        view.Path = new List<(int X, int Y)>();
        _playerPosition = (0, 0);
        view.PlayerPosition = _playerPosition;
        _lastClickedCell = null;
        _targetPosition = null;
        
        if (RunAiButton is not null)
        {
            RunAiButton.IsEnabled = false;
        }
        
        UpdateStatusText();
    }

    private (int X, int Y)? _lastClickedCell;
    private bool _isDragging;

    private void OnMazeCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (MazeCanvas is not MazeView view || view.Maze is null || _isFinished || _isAiRunning)
            return;

        var point = e.GetPosition(MazeCanvas);
        var cell = view.GetCellFromPoint(point);
        
        if (cell is null)
            return;

        var (targetX, targetY) = cell.Value;

        if (MouseControlCheckBox?.IsChecked == true && e.ClickCount == 2)
        {
            var path = _solver.FindPath(view.Maze, _playerPosition.X, _playerPosition.Y, targetX, targetY);
            if (path.Count > 1 && path.Last() == (targetX, targetY))
            {
                var nextStep = path[1];
                if (view.Maze.CanMove(_playerPosition.X, _playerPosition.Y, 
                    nextStep.X - _playerPosition.X, nextStep.Y - _playerPosition.Y))
                {
                    _playerPosition = nextStep;
                    view.PlayerPosition = _playerPosition;
                    _steps++;
                    
                    if (ShowHintsCheckBox?.IsChecked == true)
                    {
                        view.Path = new List<(int X, int Y)>();
                    }
                    UpdateStatusText();
                }
            }
            return;
        }

        if (ShowHintsCheckBox?.IsChecked == true)
        {
            ShowHintPath(view, targetX, targetY);
            _lastClickedCell = (targetX, targetY);
        }

        if (MouseControlCheckBox?.IsChecked == true)
        {
            _isDragging = true;
        }
    }

    private void OnMazeCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || MazeCanvas is not MazeView view || view.Maze is null || _isFinished || _isAiRunning)
            return;

        var point = e.GetPosition(MazeCanvas);
        var cell = view.GetCellFromPoint(point);
        
        if (cell is null)
            return;

        var (targetX, targetY) = cell.Value;

        if (ShowHintsCheckBox?.IsChecked == true)
        {
            ShowHintPath(view, targetX, targetY);
        }
    }

    private void OnMazeCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || MazeCanvas is not MazeView view || view.Maze is null || _isFinished || _isAiRunning)
        {
            _isDragging = false;
            return;
        }

        var point = e.GetPosition(MazeCanvas);
        var cell = view.GetCellFromPoint(point);
        
        if (cell is null)
        {
            _isDragging = false;
            return;
        }

        var (targetX, targetY) = cell.Value;

        var path = _solver.FindPath(view.Maze, _playerPosition.X, _playerPosition.Y, targetX, targetY);
        if (path.Count > 1 && path.Last() == (targetX, targetY))
        {
            var nextStep = path[1];
            if (view.Maze.CanMove(_playerPosition.X, _playerPosition.Y, 
                nextStep.X - _playerPosition.X, nextStep.Y - _playerPosition.Y))
            {
                _playerPosition = nextStep;
                view.PlayerPosition = _playerPosition;
                _steps++;
                
                if (ShowHintsCheckBox?.IsChecked == true)
                {
                    view.Path = new List<(int X, int Y)>();
                }
                UpdateStatusText();
            }
        }

        _isDragging = false;
    }

    private async Task RunAiToTarget(MazeView view, int targetX, int targetY)
    {
        if (view.Maze is null || _isAiRunning)
            return;

        _isAiRunning = true;
        _targetPosition = (targetX, targetY);

        var path = _solver.FindPath(view.Maze, _playerPosition.X, _playerPosition.Y, targetX, targetY);
        
        if (path.Count > 0 && path.Last() == (targetX, targetY))
        {
            view.Path = path;
            
            // ИИ
            foreach (var (x, y) in path.Skip(1))
            {
                _playerPosition = (x, y);
                view.PlayerPosition = _playerPosition;
                _steps++;
                await Task.Delay(50); // Задержка для анимации
            }

            UpdateStatusText();
        }

        _isAiRunning = false;
    }

    private void ShowHintPath(MazeView view, int targetX, int targetY)
    {
        if (view.Maze is null)
            return;

        var path = _solver.FindPath(view.Maze, _playerPosition.X, _playerPosition.Y, targetX, targetY);
        
        if (path.Count > 0 && path.Last() == (targetX, targetY))
        {
            view.Path = path;
            _targetPosition = (targetX, targetY);
            _lastClickedCell = (targetX, targetY);
            
            if (RunAiButton is not null)
            {
                RunAiButton.IsEnabled = true;
            }
        }
    }

    private async void OnRunAiToTarget(object? sender, RoutedEventArgs e)
    {
        if (MazeCanvas is not MazeView view || view.Maze is null || _isFinished || _isAiRunning)
            return;

        var (targetX, targetY) = _lastClickedCell ?? (view.Maze.Width - 1, view.Maze.Height - 1);
        
        await RunAiToTarget(view, targetX, targetY);
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (TryHandleMovement(e.Key))
            e.Handled = true;
    }

    private bool TryHandleMovement(Key key)
    {
        if (_isFinished || _isAiRunning)
            return false;

        (int dx, int dy)? direction = key switch
        {
            Key.Left or Key.A => (-1, 0),
            Key.Right or Key.D => (1, 0),
            Key.Up or Key.W => (0, -1),
            Key.Down or Key.S => (0, 1),
            _ => null
        };

        if (direction is null || MazeCanvas?.Maze is not { } maze)
            return false;

        var (dx, dy) = direction.Value;
        if (!maze.CanMove(_playerPosition.X, _playerPosition.Y, dx, dy))
            return false;

        _playerPosition = (_playerPosition.X + dx, _playerPosition.Y + dy);
        MazeCanvas.PlayerPosition = _playerPosition;
        _steps++;

        // Очищаем подсказку при движении
        if (ShowHintsCheckBox?.IsChecked == true && MazeCanvas.Path.Count > 0)
        {
            MazeCanvas.Path = new List<(int X, int Y)>();
            // Деактивируем кнопку ИИ при движении
            if (RunAiButton is not null)
            {
                RunAiButton.IsEnabled = false;
            }
        }

        UpdateStatusText();
        return true;
    }

    private void UpdateStatusText()
    {
        if (StatusText is null || StatsText is null)
            return;

        if (MazeCanvas?.Maze is not { } maze)
        {
            StatusText.Text = "Сначала сгенерируйте лабиринт";
            StatsText.Text = "";
            return;
        }

        var elapsed = _gameTimer.Elapsed;
        StatsText.Text = $"Шагов: {_steps} | Время: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

        if (_playerPosition.X == maze.Width - 1 && _playerPosition.Y == maze.Height - 1)
        {
            if (!_isFinished)
            {
                _isFinished = true;
                _gameTimer.Stop();
                
                StatusText.Text = "🎉 Финиш! Поздравляем!";
                
                var entry = new LeaderboardEntry
                {
                    PlayerName = $"Игрок {_leaderboard.Count + 1}",
                    Time = _gameTimer.Elapsed,
                    Steps = _steps,
                    Width = maze.Width,
                    Height = maze.Height
                };
                
                _leaderboard.Add(entry);
                _leaderboard.Sort((a, b) => 
                {
                    var timeCompare = a.Time.CompareTo(b.Time);
                    return timeCompare != 0 ? timeCompare : a.Steps.CompareTo(b.Steps);
                });

                UpdateLeaderboard();
            }
        }
        else
        {
            if (_isAiRunning)
                StatusText.Text = "ИИ проходит лабиринт...";
            else if (_lastClickedCell.HasValue)
                StatusText.Text = $"Кликните на место в лабиринте, чтобы увидеть путь. Нажмите кнопку ИИ для автоматического прохождения.";
            else
                StatusText.Text = "Управление: стрелки/WASD. Кликните на место в лабиринте, чтобы увидеть путь до него.";
        }
    }

    private void UpdateLeaderboard()
    {
        if (LeaderboardList is null)
            return;

        var topEntries = _leaderboard.Take(10).ToList();
        LeaderboardList.ItemsSource = topEntries;
    }
}
