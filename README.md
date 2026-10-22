# MarkdownUtil 4.9.0: the bridge to mdu

This is the last release of the `MarkdownUtil` package. Its code is unchanged from [MarkdownUtil 4.8.0](https://github.com/capjan/mdu/releases/tag/v4.8.0). Rendering, linting, and every other command work exactly as before, so pipelines keep running.

The release adds one thing: a notice that tells you MarkdownUtil has been replaced by [mdu](https://github.com/capjan/mdu) and offers to make the move for you.

## What you see

| Situation | Output | Exit code |
| --- | --- | --- |
| Every run | One `warning:` line on stderr | Unchanged |
| CI with GitHub Actions | Also a `::warning` annotation on stdout, visible in the run summary | Unchanged |
| CI with Azure DevOps | Also a `##vso[task.logissue]` issue on stdout | Unchanged |
| Interactive terminal | The steps, then `Run these steps now? [Y/n]` | Unchanged |

Answering `n` continues with MarkdownUtil. Answering `y` runs the steps:

```bash
dotnet tool uninstall --global MarkdownUtil
dotnet tool install --global mdu
```

The steps are printed before they run, so you can run them yourself if something goes wrong.

## Why the notice does not break pipelines

The notice runs from a module initializer, before the command handling in `Program.cs`. It only ever writes to stderr or to a stdout annotation. It never changes an exit code and never prompts when input or output is redirected or a CI system is detected.

## Migrating by hand

```bash
dotnet tool uninstall --global MarkdownUtil
dotnet tool install --global mdu
```

In a project with a tool manifest:

```bash
dotnet tool uninstall MarkdownUtil
dotnet tool install mdu
```

## Changes compared to 4.8.0

- `Source/MarkdownUtil/Migration/`: the notice, the CI detection, and the migration steps.
- `Source/MarkdownUtilTest/MigrationNoticeTest.cs`: tests for the notice.
- Nothing else. Compare with `git diff v4.8.0 -- Source` in [capjan/mdu](https://github.com/capjan/mdu) to verify.

## Building

```bash
dotnet build MarkdownUtils.sln
dotnet test MarkdownUtils.sln
dotnet pack Source/MarkdownUtil/MarkdownUtil.csproj --configuration Release -p:Version=4.9.0
```

The version is set at pack time, as in the upstream project.
