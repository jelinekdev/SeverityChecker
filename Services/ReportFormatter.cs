using System.Net;
using System.Text;
using SeverityChecker.Models;

namespace SeverityChecker.Services;

public static class ReportFormatter
{
    public static string BuildSubject(ScanReport report) =>
        $"[SeverityChecker] {report.Findings.Count} new vulnerabilit{(report.Findings.Count == 1 ? "y" : "ies")} found";

    public static string ToPlainText(ScanReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Scan time (UTC): {report.ScannedAtUtc:u}");
        builder.AppendLine($"Dependencies checked: {report.ScannedDependencyCount}");
        builder.AppendLine($"New findings: {report.Findings.Count}");
        builder.AppendLine();

        foreach (var finding in OrderBySeverity(report.Findings))
        {
            builder.AppendLine($"[{finding.Vulnerability.Severity}] {finding.Dependency.Name} {finding.Dependency.Version}");
            builder.AppendLine($"  {finding.Vulnerability.CveId ?? finding.Vulnerability.Id}: {finding.Vulnerability.Summary}");
            builder.AppendLine($"  {finding.Vulnerability.Url}");
            builder.AppendLine($"  Used in: {string.Join(", ", finding.Dependency.Sources)}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static string ToHtml(ScanReport report)
    {
        var builder = new StringBuilder();
        builder.Append("<h2>SeverityChecker report</h2>");
        builder.Append($"<p>Scan time (UTC): {report.ScannedAtUtc:u}<br/>");
        builder.Append($"Dependencies checked: {report.ScannedDependencyCount}<br/>");
        builder.Append($"New findings: {report.Findings.Count}</p>");
        builder.Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\" style=\"border-collapse:collapse;font-family:sans-serif;font-size:13px\">");
        builder.Append("<tr><th>Severity</th><th>Package</th><th>Version</th><th>Vulnerability</th><th>Summary</th><th>Used in</th></tr>");
        foreach (var finding in OrderBySeverity(report.Findings))
        {
            var vulnerability = finding.Vulnerability;
            var label = WebUtility.HtmlEncode(vulnerability.CveId ?? vulnerability.Id);
            builder.Append("<tr>");
            builder.Append($"<td>{vulnerability.Severity}</td>");
            builder.Append($"<td>{WebUtility.HtmlEncode(finding.Dependency.Name)}</td>");
            builder.Append($"<td>{WebUtility.HtmlEncode(finding.Dependency.Version)}</td>");
            builder.Append($"<td><a href=\"{WebUtility.HtmlEncode(vulnerability.Url)}\">{label}</a></td>");
            builder.Append($"<td>{WebUtility.HtmlEncode(vulnerability.Summary)}</td>");
            builder.Append($"<td>{WebUtility.HtmlEncode(string.Join(", ", finding.Dependency.Sources))}</td>");
            builder.Append("</tr>");
        }
        builder.Append("</table>");
        return builder.ToString();
    }

    private static IEnumerable<Finding> OrderBySeverity(IEnumerable<Finding> findings) =>
        findings
            .OrderByDescending(finding => finding.Vulnerability.Severity)
            .ThenBy(finding => finding.Dependency.Name, StringComparer.OrdinalIgnoreCase);
}
