using System.Text.Json.Serialization;

namespace SeverityChecker.Infrastructure.Osv.Models;

internal sealed record OsvRequest(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("package")] OsvPackage Package
);

internal sealed record OsvPackage(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ecosystem")] string Ecosystem
);