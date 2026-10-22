using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sunset;

public sealed record NoticeConfig(
    string Mode,
    NoticeSection Sunset,
    NoticeSection Sunrise)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>The section shown by this release: sunrise when the mode says so, otherwise sunset.</summary>
    public NoticeSection Active => string.Equals(Mode, "sunrise", StringComparison.OrdinalIgnoreCase) ? Sunrise : Sunset;

    public bool IsSunrise => ReferenceEquals(Active, Sunrise);

    public static NoticeConfig Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("notice.json")
            ?? throw new InvalidOperationException("The embedded notice.json resource is missing.");
        return Parse(stream);
    }

    public static NoticeConfig Parse(Stream json) =>
        JsonSerializer.Deserialize<NoticeConfig>(json, Options)
        ?? throw new InvalidDataException("notice.json is empty.");
}

public sealed record NoticeSection(
    string Title,
    string CiHint,
    string Scroller,
    IReadOnlyList<string> Lines,
    IReadOnlyList<NoticeLink> Links);

public sealed record NoticeLink(string Label, string Url);
