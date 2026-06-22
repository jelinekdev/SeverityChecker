using System.Text.Json.Serialization;

namespace SeverityChecker.Infrastructure.Osv.Models;

internal sealed record OsvResponse(
    [property: JsonPropertyName("vulns")] IReadOnlyList<OsvVulnerability>? Vulnerabilities
);

internal sealed record OsvVulnerability(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("severity")] IReadOnlyList<OsvSeverity>? Severity,
    [property: JsonPropertyName("affected")] IReadOnlyList<OsvAffected>? Affected,
    [property: JsonPropertyName("references")] IReadOnlyList<OsvReference>? References
);

internal sealed record OsvSeverity(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("score")] string Score
);

internal sealed record OsvAffected(
    [property: JsonPropertyName("ranges")] IReadOnlyList<OsvRange>? Ranges
);

internal sealed record OsvRange(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("events")] IReadOnlyList<OsvEvent>? Events
);

internal sealed record OsvEvent(
    [property: JsonPropertyName("fixed")] string? Fixed
);

internal sealed record OsvReference(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url
);