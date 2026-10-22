namespace Sunset;

/// <summary>Builds the ASCII sky frames for the sunset and sunrise animations.</summary>
public static class SkyAnimation
{
    public const int Width = 56;
    public const int Height = 12;

    private const int Horizon = 10;        // ground line
    private const int RidgeRow = Horizon - 1;
    private const int SunTop = 1;
    private const int SunHidden = Horizon + 1;
    private const int Steps = 24;

    // Fixed star positions for the night sky of the sunrise animation.
    private static readonly (int X, int Y)[] Stars =
    [
        (4, 1), (12, 3), (20, 0), (33, 4), (41, 1), (49, 3), (8, 5), (26, 6), (46, 6),
    ];

    /// <summary>Returns one frame per step, each frame made of <see cref="Height"/> lines.</summary>
    public static IReadOnlyList<string[]> Frames(bool rising)
    {
        var frames = new List<string[]>(Steps + 1);
        for (var step = 0; step <= Steps; step++)
        {
            var progress = (double)step / Steps;                   // 0 = sun on top, 1 = hidden
            var sunProgress = rising ? 1 - progress : progress;
            var sunRow = (int)Math.Round(SunTop + sunProgress * (SunHidden - SunTop));
            frames.Add(Render(sunRow, rising));
        }
        return frames;
    }

    private static string[] Render(int sunRow, bool rising)
    {
        var grid = new char[Height][];
        for (var row = 0; row < Height; row++)
            grid[row] = Enumerable.Repeat(' ', Width).ToArray();

        if (rising)
        {
            foreach (var (x, y) in Stars)
                grid[y][x] = '*';
        }

        DrawSun(grid, Width / 2, sunRow);
        DrawGround(grid);

        return grid.Select(row => new string(row).TrimEnd()).ToArray();
    }

    private static void DrawSun(char[][] grid, int centerX, int centerRow)
    {
        // The sun is clipped at the ridge, so it looks like it sets behind the mountains.
        void Put(int x, int y, char c)
        {
            if (y >= 0 && y < RidgeRow && x >= 0 && x < Width)
                grid[y][x] = c;
        }

        Put(centerX - 2, centerRow - 1, '\\');
        Put(centerX, centerRow - 1, '|');
        Put(centerX + 2, centerRow - 1, '/');

        Put(centerX - 2, centerRow, '-');
        Put(centerX - 1, centerRow, '(');
        Put(centerX, centerRow, 'O');
        Put(centerX + 1, centerRow, ')');
        Put(centerX + 2, centerRow, '-');

        Put(centerX - 2, centerRow + 1, '/');
        Put(centerX, centerRow + 1, '|');
        Put(centerX + 2, centerRow + 1, '\\');
    }

    private static void DrawGround(char[][] grid)
    {
        for (var x = 0; x < Width; x++)
        {
            grid[RidgeRow][x] = x % 2 == 0 ? '/' : '\\';
            grid[Horizon][x] = '_';
        }
    }
}
