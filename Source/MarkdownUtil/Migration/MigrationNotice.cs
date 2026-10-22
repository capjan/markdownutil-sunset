using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MarkdownUtil.Migration;

/// <summary>
/// End-of-life notice of MarkdownUtil. It runs before the regular command, so the command handling
/// in Program.cs stays unchanged and the tool keeps working in every pipeline.
/// </summary>
public static class MigrationNotice
{
    public const string Title = "MarkdownUtil is end of life";
    public const string Uninstall = "dotnet tool uninstall --global MarkdownUtil";
    public const string Install = "dotnet tool install --global mdu";
    public const string LocalUninstall = "dotnet tool uninstall MarkdownUtil";
    public const string LocalInstall = "dotnet tool install mdu";

    public static readonly string WarningMessage =
        $"MarkdownUtil has been replaced by mdu. Replace it with: {Uninstall} && {Install}";

    [ModuleInitializer]
    internal static void ShowOnStartup()
    {
        var ci = CiEnvironment.Detect();

        WriteWarnings(ci, Console.Out, Console.Error);

        var canPrompt = ci == CiProvider.None && !Console.IsInputRedirected && !Console.IsOutputRedirected;
        if (!canPrompt)
            return;

        var global = IsGlobalInstall();
        if (!global)
        {
            Console.Error.WriteLine($"This MarkdownUtil is not a global tool. In the project that uses it, run: {LocalUninstall} && {LocalInstall}");
            return;
        }

        Console.Out.Write(Plan(global: true));
        Console.Out.Write("Run these steps now? [Y/n] ");

        var answer = Console.ReadLine()?.Trim();
        if (answer is "n" or "N" or "no" or "No" or "NO")
        {
            Console.Out.WriteLine("Continuing with MarkdownUtil. Run the steps above later to move to mdu.");
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            // Windows locks the running tool, so the steps run after this process has exited.
            StartMigration(Environment.ProcessId);
            Console.Out.WriteLine("Migration started. Run your command again once the steps above have finished.");
            Environment.Exit(0);
        }

        // Elsewhere the running tool's files can be replaced, so the steps run here, in the foreground.
        var migrated = RunStep("tool", "uninstall", "--global", "MarkdownUtil") && RunStep("tool", "install", "--global", "mdu");
        Console.Out.WriteLine(migrated
            ? "mdu is installed. Run your command again with: mdu"
            : $"Migration failed. Run these steps manually: {Uninstall} and {Install}");
        Environment.Exit(migrated ? 0 : 1);
    }

    private static bool RunStep(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start);
        if (process is null)
            return false;

        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Writes the warning in the format the CI system understands, plus a plain line for every other run.</summary>
    public static void WriteWarnings(CiProvider ci, TextWriter stdout, TextWriter stderr)
    {
        stderr.WriteLine($"warning: {WarningMessage}");

        switch (ci)
        {
            case CiProvider.GitHubActions:
                // Workflow commands on stdout become annotations in the GitHub Actions UI.
                stdout.WriteLine($"::warning title={Title}::{WarningMessage}");
                break;
            case CiProvider.AzureDevOps:
                stdout.WriteLine($"##vso[task.logissue type=warning]{WarningMessage}");
                break;
        }
    }

    public static string Plan(bool global)
    {
        var uninstall = global ? Uninstall : LocalUninstall;
        var install = global ? Install : LocalInstall;

        var lines = new[]
        {
            "MarkdownUtil has been replaced by mdu. This is the last MarkdownUtil release.",
            "We are sorry for the extra step. The move takes two commands:",
            "",
            $"  1. {uninstall}",
            $"  2. {install}",
            "",
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    /// <summary>
    /// Builds the Windows helper that waits for this process to exit, then runs the planned steps.
    /// The wait is needed because Windows locks the running tool's files.
    /// </summary>
    public static ProcessStartInfo MigrationHelper(int parentProcessId)
    {
        var script = $"try {{ Wait-Process -Id {parentProcessId} -ErrorAction SilentlyContinue }} catch {{}}; "
                   + $"{Uninstall}; if ($LASTEXITCODE -eq 0) {{ {Install} }}; "
                   + "if ($LASTEXITCODE -eq 0) { Write-Output 'mdu is installed. Run: mdu --version' } "
                   + $"else {{ Write-Output 'Migration failed. Run these steps manually: {Uninstall} and {Install}' }}";

        var start = new ProcessStartInfo("powershell") { UseShellExecute = false };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-Command");
        start.ArgumentList.Add(script);
        return start;
    }

    private static void StartMigration(int parentProcessId)
    {
        // The helper inherits the console, so its output appears after this process has exited.
        Process.Start(MigrationHelper(parentProcessId));
    }

    private static bool IsGlobalInstall()
    {
        try
        {
            var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, UseShellExecute = false };
            start.ArgumentList.Add("tool");
            start.ArgumentList.Add("list");
            start.ArgumentList.Add("--global");

            using var process = Process.Start(start);
            if (process is null)
                return false;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Split('\n').Any(line => line.TrimStart().StartsWith("markdownutil ", StringComparison.OrdinalIgnoreCase));
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }
}
