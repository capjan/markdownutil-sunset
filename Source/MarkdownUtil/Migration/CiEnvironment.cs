namespace MarkdownUtil.Migration;

public enum CiProvider
{
    None,
    GitHubActions,
    AzureDevOps,
    Other,
}

/// <summary>Detects whether the tool runs in a continuous integration pipeline and which one.</summary>
public static class CiEnvironment
{
    private static readonly string[] OtherMarkers =
    [
        "CI",
        "CONTINUOUS_INTEGRATION",
        "GITLAB_CI",
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

    public static CiProvider Detect(Func<string, string?>? environment = null)
    {
        environment ??= Environment.GetEnvironmentVariable;

        if (IsSet(environment("GITHUB_ACTIONS")))
            return CiProvider.GitHubActions;

        if (IsSet(environment("TF_BUILD")))
            return CiProvider.AzureDevOps;

        return OtherMarkers.Any(name => IsSet(environment(name)))
            ? CiProvider.Other
            : CiProvider.None;
    }

    private static bool IsSet(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value is not "false" and not "0";
}
