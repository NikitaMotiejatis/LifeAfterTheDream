using Microsoft.AspNetCore.Mvc;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/ais")]
public class AisController : ControllerBase
{
    private readonly IHttpClientFactory _http;

    public AisController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public async Task<IActionResult> GetTargets()
    {
        var client = _http.CreateClient();
        var response = await client.GetAsync("https://klaipedatraffic.lt/ais/targets.geojson");
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }
}
