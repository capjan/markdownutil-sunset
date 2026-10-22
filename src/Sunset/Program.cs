using Sunset;

var config = NoticeConfig.Load();
var section = config.Active;

// In a pipeline, print the notice to stderr and fail, so the move cannot be missed.
if (CiEnvironment.IsCi())
{
    Console.Error.WriteLine(section.CiHint);
    return 1;
}

var interactive = !Console.IsOutputRedirected;

if (interactive)
{
    Console.CancelKeyPress += (_, _) => Console.Write("\x1b[?25h\x1b[0m");
    var color = Environment.GetEnvironmentVariable("NO_COLOR") is null;
    Presenter.PlayAnimation(Console.Out, section, config.IsSunrise, color);
}

Presenter.WriteNotice(section, Console.Out, hyperlinks: interactive);
return 0;
