using System.Text.Json.Serialization;

namespace LogDbConnsMetricCollector.Api.Models;

public class MetricResponse
{
    [JsonPropertyName("metric")]
    public MetricInfo Metric { get; set; } = new();

    [JsonPropertyName("measurement")]
    public Measurement Measurement { get; set; } = new();

    [JsonPropertyName("rawLogs")]
    public string RawLogs { get; set; } = string.Empty;
}

public class MetricInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("collectorStrategy")]
    public string CollectorStrategy { get; set; } = string.Empty;
}

public class Measurement
{
    [JsonPropertyName("apiIdentifier")]
    public string ApiIdentifier { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("uniqueConnections")]
    public IEnumerable<string> UniqueConnections { get; set; } = [];
}
