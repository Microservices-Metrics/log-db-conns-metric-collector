using LogDbConnsMetricCollector.Api.Models;
using LogDbConnsMetricCollector.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogDbConnsMetricCollector.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CollectController : ControllerBase
{
    private readonly LogDbConnsMetricCollectorService _logDbConnsService;
    private readonly ILogger<CollectController> _logger;

    public CollectController(
        LogDbConnsMetricCollectorService logDbConnsService,
        ILogger<CollectController> logger)
    {
        _logDbConnsService = logDbConnsService;
        _logger = logger;
    }

    /// <summary>
    /// Fetches logs from the given URL and returns the count of unique database connections found.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MetricResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Collect([FromBody] CollectorBody collectorBody)
    {
        _logger.LogInformation("POST /api/collect received. UrlLog={UrlLog}, DbPattern={DbPattern}",
            collectorBody.UrlMicroserviceLog, collectorBody.DbPattern ?? "(default)");

        if (string.IsNullOrWhiteSpace(collectorBody.UrlMicroserviceLog))
        {
            _logger.LogWarning("Request rejected: 'urlLog' field is missing or empty.");
            return BadRequest("The 'urlLog' field is required.");
        }

        var result = await _logDbConnsService.CollectAsync(collectorBody.UrlMicroserviceLog, collectorBody.DbPattern);

        _logger.LogInformation("Response sent. UniqueConnections={Count}", result.Measurement.Value);

        return Ok(result);
    }
}
