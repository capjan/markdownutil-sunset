namespace Sunset;

/// <summary>Detects whether the tool runs inside a continuous integration pipeline.</summary>
public static class CiEnvironment
{
    // Variables that CI systems set. Any of them being present counts as CI.
    private static readonly string[] Markers =
    [
        "CI",
        "CONTINUOUS_INTEGRATION",
        "BUILD_NUMBER",
        "RUN_ID",
        "GITHUB_ACTIONS",
        "GITLAB_CI",
        "TF_BUILD",
        "JENKINS_URL",
        "TEAMCITY_VERSION",
        "CIRCLECI",
        "BUILDKITE",
        "APPVEYOR",
        "BITBUCKET_BUILD_NUMBER",
        "DRONE",
        "CODEBUILD_BUILD_ID",
        "TRAVIS",
    ];

    public static bool IsCi(Func<string, string?>? environment = null)
    {
        environment ??= Environment.GetEnvironmentVariable;
        return Markers.Any(name => !string.IsNullOrWhiteSpace(environment(name)) && environment(name) is not "false" and not "0");
    }
}
