# MarkdownUtil sunset

Final release of the `MarkdownUtil` package. Its `mdu` command shows an animated end-of-life notice and explains the migration to the [mdu](https://github.com/capjan/mdu) package.

## Behavior

| Environment | Animation | Output | Exit code |
| --- | --- | --- | --- |
| Interactive terminal | Demoscene-style sunset (or sunrise): plasma, retro grid, raster sun, sine scroller | Notice with clickable links | 0 |
| Pipe or redirected output | none | Notice as plain text | 0 |
| CI pipeline (`CI`, `GITHUB_ACTIONS`, `GITLAB_CI`, …) | none | Short hint on stderr | 1 |

The CI exit code is deliberate. A failing pipeline shows the migration, and the apology in the notice explains why.

## Configuration

All text lives in `src/Sunset/Res/notice.json`:

- `mode`: `sunset` (default for 4.9.0) or `sunrise`. Sunrise replaces the sunset notice in a successor release.
- `scroller`: the text of the sine scroller under the animation. Keep it short and uppercase-friendly.
- `sunset` and `sunrise`: `title`, `ciHint`, `lines` (plain text), and `links` (`label`, `url`). Links are clickable in terminals that support OSC 8 hyperlinks and are always printed as text.

Changing the text means changing the JSON and publishing a new version.

## Build and test

```bash
dotnet build MarkdownUtilSunset.slnx
dotnet test MarkdownUtilSunset.slnx
```

Pack the release with the version supplied at pack time, so the version is not hard-coded:

```bash
dotnet pack src/Sunset/Sunset.csproj --configuration Release -p:Version=4.9.0
```
