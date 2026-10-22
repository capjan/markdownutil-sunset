using MarkdownUtil.Migration;
using Xunit;

namespace MarkdownUtilTest;

public class MigrationNoticeTest
{
    [Fact]
    public void DetectsGitHubActions()
    {
        Assert.Equal(CiProvider.GitHubActions, CiEnvironment.Detect(name => name == "GITHUB_ACTIONS" ? "true" : null));
    }

    [Fact]
    public void DetectsAzureDevOps()
    {
        Assert.Equal(CiProvider.AzureDevOps, CiEnvironment.Detect(name => name == "TF_BUILD" ? "True" : null));
    }

    [Fact]
    public void DetectsOtherCiSystems()
    {
        Assert.Equal(CiProvider.Other, CiEnvironment.Detect(name => name == "GITLAB_CI" ? "true" : null));
    }

    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    [InlineData("")]
    public void IgnoresFalseAndEmptyValues(string value)
    {
        Assert.Equal(CiProvider.None, CiEnvironment.Detect(name => name == "CI" ? value : null));
    }

    [Fact]
    public void WarnsOnStderrOnEveryRunAndAddsGitHubAnnotationOnStdout()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        MigrationNotice.WriteWarnings(CiProvider.GitHubActions, stdout, stderr);

        Assert.StartsWith("warning: MarkdownUtil has been replaced by mdu", stderr.ToString());
        Assert.StartsWith("::warning title=MarkdownUtil is end of life::", stdout.ToString());
    }

    [Fact]
    public void WritesAzureDevOpsIssueOnStdout()
    {
        var stdout = new StringWriter();

        MigrationNotice.WriteWarnings(CiProvider.AzureDevOps, stdout, new StringWriter());

        Assert.StartsWith("##vso[task.logissue type=warning]MarkdownUtil has been replaced", stdout.ToString());
    }

    [Fact]
    public void PlainRunWritesOnlyTheStderrWarning()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        MigrationNotice.WriteWarnings(CiProvider.None, stdout, stderr);

        Assert.Empty(stdout.ToString());
        Assert.Contains("dotnet tool install --global mdu", stderr.ToString());
    }

    [Fact]
    public void PlanListsBothGlobalStepsInOrder()
    {
        var plan = MigrationNotice.Plan(global: true);

        var uninstall = plan.IndexOf(MigrationNotice.Uninstall, StringComparison.Ordinal);
        var install = plan.IndexOf(MigrationNotice.Install, StringComparison.Ordinal);
        Assert.True(uninstall >= 0);
        Assert.True(install > uninstall);
    }

    [Fact]
    public void PlanForLocalToolUsesLocalCommands()
    {
        var plan = MigrationNotice.Plan(global: false);

        Assert.Contains(MigrationNotice.LocalUninstall, plan);
        Assert.Contains(MigrationNotice.LocalInstall, plan);
    }

    [Fact]
    public void WindowsHelperWaitsForTheParentBeforeMigrating()
    {
        var start = MigrationNotice.MigrationHelper(4242);

        Assert.Equal("powershell", start.FileName);
        Assert.Contains(start.ArgumentList, argument => argument.Contains("Wait-Process -Id 4242", StringComparison.Ordinal));
        Assert.Contains(start.ArgumentList, argument => argument.Contains(MigrationNotice.Install, StringComparison.Ordinal));
    }
}
