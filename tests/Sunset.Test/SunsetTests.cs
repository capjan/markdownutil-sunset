using System.Text;
using Sunset;
using Xunit;

namespace Sunset.Test;

public class CiEnvironmentTests
{
    [Fact]
    public void DetectsKnownCiVariable()
    {
        Assert.True(CiEnvironment.IsCi(name => name == "GITHUB_ACTIONS" ? "true" : null));
    }

    [Fact]
    public void IgnoresEmptyAndFalseValues()
    {
        Assert.False(CiEnvironment.IsCi(name => name == "CI" ? "false" : null));
        Assert.False(CiEnvironment.IsCi(name => name == "CI" ? "" : null));
    }

    [Fact]
    public void NoVariablesMeansNoCi()
    {
        Assert.False(CiEnvironment.IsCi(_ => null));
    }
}

public class NoticeConfigTests
{
    [Fact]
    public void EmbeddedConfigDefaultsToSunset()
    {
        var config = NoticeConfig.Load();

        Assert.False(config.IsSunrise);
        Assert.Equal("We moved! MarkdownUtil is now mdu", config.Active.Title);
        Assert.Contains("dotnet tool install --global mdu", string.Join("\n", config.Active.Lines));
    }

    [Fact]
    public void SunriseModeSelectsSunriseSection()
    {
        var json = """
            {
              "mode": "sunrise",
              "sunset": { "title": "Old", "ciHint": "old", "lines": [], "links": [] },
              "sunrise": { "title": "New", "ciHint": "new", "lines": ["hello"], "links": [] }
            }
            """;

        var config = NoticeConfig.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)));

        Assert.True(config.IsSunrise);
        Assert.Equal("New", config.Active.Title);
    }
}

public class DemosceneTests
{
    private static readonly System.Text.RegularExpressions.Regex Ansi = new("\x1b\\[[0-9;?]*[A-Za-z]|\x1b\\].*?\x1b\\\\");

    [Fact]
    public void ProducesOneFramePerStepPlusFinal()
    {
        var frames = Demoscene.Frames("HELLO", rising: false, color: true);

        Assert.Equal(Demoscene.Steps + 1, frames.Count);
    }

    [Fact]
    public void EveryFrameHasTheConfiguredRowCountAndWidth()
    {
        foreach (var frame in Demoscene.Frames("HELLO", rising: false, color: false))
        {
            var rows = frame.Split('\n');
            Assert.Equal(Demoscene.Height, rows.Length);
            Assert.All(rows, row => Assert.Equal(Demoscene.Width, row.Length));
        }
    }

    [Fact]
    public void SunsetStartsWithSunInTheSkyAndEndsBehindTheHorizon()
    {
        var frames = Demoscene.Frames("HELLO", rising: false, color: false);

        Assert.Contains('\u2588', frames[0]);
        Assert.DoesNotContain('\u2588', frames[^1].Split('\n')[..5].Aggregate("", (acc, row) => acc + row));
    }

    [Fact]
    public void SunriseStartsWithoutSunAndEndsWithSunInTheSky()
    {
        var frames = Demoscene.Frames("HELLO", rising: true, color: false);

        Assert.DoesNotContain('\u2588', frames[0].Split('\n')[..5].Aggregate("", (acc, row) => acc + row));
        Assert.Contains('\u2588', frames[^1]);
    }

    [Fact]
    public void ColorOffProducesNoEscapeSequences()
    {
        var frame = Demoscene.Frames("HELLO", rising: false, color: false)[10];

        Assert.DoesNotContain('\x1b', frame);
    }

    [Fact]
    public void ColorOnProducesAnsiColorsThatStripToTheSameText()
    {
        var colored = Demoscene.Frames("HELLO", rising: false, color: true)[10];
        var plain = Demoscene.Frames("HELLO", rising: false, color: false)[10];

        Assert.Contains("\x1b[38;5;", colored);
        Assert.Equal(plain, Ansi.Replace(colored, ""));
    }
}
