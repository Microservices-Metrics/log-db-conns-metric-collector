using System.Text.RegularExpressions;
using LogDbConnsMetricCollector.Api.Models;

namespace LogDbConnsMetricCollector.Api.Services;

public class LogDbConnsMetricCollectorService
{
    // Matches EF Core patterns:
    //   Opening connection to database 'MyDb' on server 'localhost'.
    //   Opened connection to database 'MyDb' on server 'tcp:myserver,1433'.
    // Also matches common ADO.NET / driver log patterns:
    //   Connected to database 'MyDb' at host 'localhost'.
    //   Connecting to host 'localhost', database 'MyDb'.
    private static readonly Regex DefaultDbPattern = new(
        @"(?:Opening|Opened|Connecting|Connected)\s+(?:connection\s+)?to\s+database\s+'(?<database>[^']+)'\s+(?:at|on)\s+(?:server|host)\s+'(?<host>[^']+)'" +
        @"|(?:Connecting|Connected)\s+to\s+(?:host|server)\s+'(?<host>[^']+)',\s+database\s+'(?<database>[^']+)'" +
        @"|(?:mongodb|postgresql|mysql|sqlserver|redis|cassandra|oracle)://(?<host>[^/\s,]+)(?:/(?<database>[^?\s]+))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly HttpClient _httpClient;
    private readonly ILogger<LogDbConnsMetricCollectorService> _logger;

    public LogDbConnsMetricCollectorService(
        HttpClient httpClient,
        ILogger<LogDbConnsMetricCollectorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MetricResponse> CollectAsync(string logUrl, string? customPattern = null)
    {
        _logger.LogInformation("Fetching logs from {LogUrl}", logUrl);

        var pattern = string.IsNullOrWhiteSpace(customPattern)
            ? DefaultDbPattern
            : new Regex(customPattern, RegexOptions.IgnoreCase);

        var logContent = await _httpClient.GetStringAsync(logUrl);

        _logger.LogInformation("Log fetched ({Length} chars). Searching for DB connection patterns.", logContent.Length);

        var uniqueConnections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in pattern.Matches(logContent))
        {
            var host = match.Groups["host"].Value.Trim();
            var database = match.Groups["database"].Value.Trim();

            var connectionKey = (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(database))
                ? $"{host}/{database}"
                : (!string.IsNullOrEmpty(host) ? host : database);

            if (!string.IsNullOrEmpty(connectionKey))
                uniqueConnections.Add(connectionKey);
        }

        _logger.LogInformation("Found {Count} unique DB connection(s): {Connections}",
            uniqueConnections.Count, string.Join(", ", uniqueConnections));

        return new MetricResponse
        {
            Metric = new MetricInfo
            {
                Name = "DB connections per service",
                CollectorStrategy = "logs"
            },
            Measurement = new Measurement
            {
                ApiIdentifier = logUrl,
                Value = uniqueConnections.Count,
                Unit = "connections",
                Timestamp = DateTime.UtcNow.ToString("o"),
                UniqueConnections = uniqueConnections
            },
            RawLogs = logContent
        };
    }
}
