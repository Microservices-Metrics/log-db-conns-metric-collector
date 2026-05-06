using System.Text.Json.Serialization;

namespace LogDbConnsMetricCollector.Api.Models;

public class CollectorBody
{
    [JsonPropertyName("urlLog")]
    public string UrlMicroserviceLog { get; set; } = string.Empty;

    /// <summary>
    /// Optional regex to identify database connection lines in the log.
    /// Must contain at least one named group: 'host' or 'database'.
    /// Default: matches EF Core and common ADO.NET connection log patterns.
    /// </summary>
    [JsonPropertyName("dbPattern")]
    public string? DbPattern { get; set; }
}
