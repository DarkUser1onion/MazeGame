using System;

namespace MazeGame.Models;

public class LeaderboardEntry
{
    public string PlayerName { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public int Steps { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    public string FormattedTime => $"{Time.Minutes:D2}:{Time.Seconds:D2}.{Time.Milliseconds:D3}";
}



