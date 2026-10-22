namespace Sunset;

/// <summary>Writes the animation and the notice to the terminal.</summary>
public static class Presenter
{
    private const string HideCursor = "\x1b[?25l";
    private const string ShowCursor = "\x1b[?25h";
    private const string SaveCursor = "\u001b7";
    private const string RestoreCursor = "\u001b8";
    private static readonly TimeSpan FrameDelay = TimeSpan.FromMilliseconds(40);
    private static readonly TimeSpan FinalHold = TimeSpan.FromMilliseconds(1200);

    public static void PlayAnimation(TextWriter output, NoticeSection section, bool rising, bool color)
    {
        var frames = Demoscene.Frames(section.Scroller, rising, color);

        output.Write(HideCursor);
        try
        {
            // Reserve the animation area below the cursor, then remember its top-left corner.
            for (var row = 0; row < Demoscene.Height; row++)
                output.WriteLine();
            output.Write($"\x1b[{Demoscene.Height}A{SaveCursor}");

            foreach (var frame in frames)
            {
                output.Write(RestoreCursor);
                output.Write(frame);
                output.Flush();
                Thread.Sleep(FrameDelay);
            }

            Thread.Sleep(FinalHold);
        }
        finally
        {
            output.Write($"{ShowCursor}\x1b[0m");
            output.WriteLine();
        }
    }

    public static void WriteNotice(NoticeSection section, TextWriter output, bool hyperlinks)
    {
        output.WriteLine();
        output.WriteLine(section.Title);
        output.WriteLine(new string('=', section.Title.Length));
        output.WriteLine();

        foreach (var line in section.Lines)
            output.WriteLine(line);

        if (section.Links.Count == 0)
            return;

        output.WriteLine();
        foreach (var link in section.Links)
        {
            // OSC 8 makes the URL clickable in terminals that support hyperlinks.
            var url = hyperlinks ? $"\x1b]8;;{link.Url}\x1b\\{link.Url}\x1b]8;;\x1b\\" : link.Url;
            output.WriteLine($"  {link.Label}: {url}");
        }
    }
}
