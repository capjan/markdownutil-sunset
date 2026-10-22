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
        Assert.Equal("MarkdownUtil has reached end of life", config.Active.Title);
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

public class SkyAnimationTests
{
    [Fact]
    public void SunsetStartsWithSunOnTopAndEndsWithoutSun()
    {
        var frames = SkyAnimation.Frames(rising: false);

        Assert.Contains('O', string.Concat(frames[0]));
        Assert.DoesNotContain('O', string.Concat(frames[^1]));
    }

    [Fact]
    public void SunriseStartsWithoutSunAndEndsWithSun()
    {
        var frames = SkyAnimation.Frames(rising: true);

        Assert.DoesNotContain('O', string.Concat(frames[0]));
        Assert.Contains('O', string.Concat(frames[^1]));
    }

    [Fact]
    public void EveryFrameHasTheConfiguredHeightAndWidth()
    {
        foreach (var frame in SkyAnimation.Frames(rising: false))
        {
            Assert.Equal(SkyAnimation.Height, frame.Length);
            Assert.All(frame, line => Assert.True(line.Length <= SkyAnimation.Width));
        }
    }
}
