using System.Text;

namespace Sunset;

/// <summary>
/// Renders demoscene-style frames: an animated plasma, a retro perspective grid,
/// a raster-striped sun that sets (or rises) and a sine-wave scroller.
/// </summary>
public static class Demoscene
{
    public const int Width = 72;
    public const int Height = 22;
    public const int Steps = 64;

    private const int Horizon = 13;
    private const double SunRadius = 6;
    private const int SunTop = 7;
    private const int SunHidden = Horizon + 8;

    private static readonly char[] Shades = " .:-=+*#%@".ToCharArray();
    private static readonly int[] PlasmaPalette = [17, 53, 90, 126, 162, 198, 204, 209, 214, 221];
    private static readonly int[] SunPalette = [226, 220, 214, 208, 202, 196, 160];
    private static readonly int[] GridPalette = [51, 45, 39, 33];
    private static readonly int[] RainbowPalette = [196, 208, 226, 46, 51, 21, 129, 201];

    private readonly record struct Cell(char Ch, int Color);

    /// <summary>Returns <see cref="Steps"/> + 1 frames. Each frame is <see cref="Height"/> rows joined by newlines.</summary>
    public static IReadOnlyList<string> Frames(string scroller, bool rising, bool color)
    {
        var text = scroller.Length == 0 ? "MDU" : scroller.ToUpperInvariant();
        var frames = new List<string>(Steps + 1);

        for (var step = 0; step <= Steps; step++)
        {
            var t = (double)step / Steps;
            var sunProgress = rising ? 1 - t : t; // 0 = top of the sky, 1 = behind the horizon
            var sunY = SunTop + sunProgress * (SunHidden - SunTop);
            var cells = Compose(t, sunY, text, step);
            frames.Add(Serialize(cells, color));
        }

        return frames;
    }

    private static Cell[,] Compose(double t, double sunY, string text, int step)
    {
        var phase = t * 2 * Math.PI;
        var cells = new Cell[Height, Width];

        DrawPlasma(cells, phase);
        DrawGrid(cells, t);
        DrawSun(cells, sunY);
        DrawScroller(cells, text, phase, step);

        return cells;
    }

    private static void DrawPlasma(Cell[,] cells, double phase)
    {
        for (var y = 0; y < Horizon; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var value = Math.Sin(x / 7.0 + phase * 2)
                          + Math.Sin(y / 3.5 + phase)
                          + Math.Sin((x + y) / 9.0 + phase * 1.5)
                          + Math.Sin(Math.Sqrt((x - Width / 2.0) * (x - Width / 2.0) + (y - Horizon) * (y - Horizon)) / 4.0 + phase);

                var level = Math.Clamp((value + 4) / 8, 0, 0.999);
                cells[y, x] = new Cell(Shades[(int)(level * Shades.Length)], PlasmaPalette[(int)(level * PlasmaPalette.Length)]);
            }
        }
    }

    private static void DrawGrid(Cell[,] cells, double t)
    {
        var scroll = (int)(t * 8);

        for (var y = Horizon; y < Height; y++)
        {
            var depth = y - Horizon + 1;
            var color = GridPalette[depth % GridPalette.Length];

            for (var x = 0; x < Width; x++)
            {
                var perspective = (x - Width / 2.0) / depth;
                var vertical = Math.Abs(Frac(perspective / 4.0)) < 0.08;
                var horizontal = (depth + scroll) % 3 == 0;

                cells[y, x] = (vertical, horizontal) switch
                {
                    (true, true) => new Cell('+', color),
                    (true, false) => new Cell('|', color),
                    (false, true) => new Cell('-', color),
                    _ => new Cell(' ', color),
                };
            }
        }

        for (var x = 0; x < Width; x++)
            cells[Horizon, x] = new Cell('=', 226);
    }

    private static void DrawSun(Cell[,] cells, double sunY)
    {
        for (var y = 0; y < Horizon; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                // Terminal cells are about twice as tall as they are wide, so the x distance is halved.
                var dx = (x - Width / 2.0) / 2;
                var dy = y - sunY;
                if (dx * dx + dy * dy > SunRadius * SunRadius)
                    continue;

                // Raster stripes: alternating two-row bars that shift the sun's colour from top to bottom.
                var stripe = Math.Clamp((int)((y - (sunY - SunRadius)) / 2), 0, SunPalette.Length - 1);
                var bar = (y / 2) % 2 == 0 ? '\u2588' : '\u2593';
                cells[y, x] = new Cell(bar, SunPalette[stripe]);
            }
        }
    }

    private static void DrawScroller(Cell[,] cells, string text, double phase, int step)
    {
        var line = text + "    ";
        var offset = step * 2;

        // Clear the band first, so the wavy letters stay readable over the grid.
        for (var y = Height - 3; y < Height; y++)
            for (var x = 0; x < Width; x++)
                cells[y, x] = new Cell(' ', 0);

        for (var x = 0; x < Width; x++)
        {
            var wave = (int)Math.Round(Math.Sin(x * 0.35 + phase * 3));
            var y = Height - 2 + wave;

            var ch = line[(x + offset) % line.Length];
            cells[y, x] = new Cell(ch, RainbowPalette[(x + step) % RainbowPalette.Length]);
        }
    }

    private static string Serialize(Cell[,] cells, bool color)
    {
        var builder = new StringBuilder(Height * Width * 4);

        for (var y = 0; y < Height; y++)
        {
            var current = -1;
            for (var x = 0; x < Width; x++)
            {
                var cell = cells[y, x];
                if (color && cell.Color != current)
                {
                    builder.Append($"\x1b[38;5;{cell.Color}m");
                    current = cell.Color;
                }

                builder.Append(cell.Ch);
            }

            if (color)
                builder.Append("\x1b[0m");

            if (y < Height - 1)
                builder.Append('\n');
        }

        return builder.ToString();
    }

    private static double Frac(double value) => value - Math.Floor(value);
}
