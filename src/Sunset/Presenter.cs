namespace Sunset;

/// <summary>Writes the animation and the notice to the terminal.</summary>
public static class Presenter
{
    private const string HideCursor = "\x1b[?25l";
    private const string ShowCursor = "\x1b[?25h";
    private const string ClearLine = "\x1b[2K";
    private static readonly TimeSpan FrameDelay = TimeSpan.FromMilliseconds(120);

    public static void PlayAnimation(TextWriter output, bool rising)
    {
        output.Write(HideCursor);
        try
        {
            var frames = SkyAnimation.Frames(rising);
            for (var index = 0; index < frames.Count; index++)
            {
                if (index > 0)
                    output.Write($"\x1b[{SkyAnimation.Height}A");

                foreach (var line in frames[index])
                    output.WriteLine($"{ClearLine}{line}");

                output.Flush();
                Thread.Sleep(FrameDelay);
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(800));
        }
        finally
        {
            output.Write(ShowCursor);
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
