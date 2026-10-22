using Sunset;

var config = NoticeConfig.Load();
var section = config.Active;

// In a pipeline, print the notice to stderr and fail, so the migration cannot be skipped.
if (CiEnvironment.IsCi())
{
    Console.Error.WriteLine(section.CiHint);
    return 1;
}

var interactive = !Console.IsOutputRedirected;

if (interactive)
{
    Console.CancelKeyPress += (_, _) => Console.Write("\x1b[?25h");
    Presenter.PlayAnimation(Console.Out, config.IsSunrise);
}

Presenter.WriteNotice(section, Console.Out, hyperlinks: interactive);
return 0;
