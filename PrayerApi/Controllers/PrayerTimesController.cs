using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/[controller]")]
public class PrayerTimesController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PrayerTimesController> _logger;

    public PrayerTimesController(IHttpClientFactory httpFactory, IMemoryCache cache, ILogger<PrayerTimesController> logger)
    {
        _httpFactory = httpFactory;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(double lat, double lon, string date = null, int method = 2)
    {
        if (double.IsNaN(lat) || double.IsNaN(lon)) return BadRequest("lat & lon required.");

        var d = string.IsNullOrEmpty(date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : date;
        var cacheKey = $"times:{lat}:{lon}:{d}:{method}";

        if (_cache.TryGetValue(cacheKey, out string cachedJson))
        {
            return Content(cachedJson, "application/json");
        }

        var client = _httpFactory.CreateClient("aladhan");
        // AlAdhan endpoint: /v1/timings/{date}?latitude=...&longitude=...&method=...
        var url = $"https://api.aladhan.com/v1/timings/{d}?latitude={lat}&longitude={lon}&method={method}";
        try
        {
            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("AlAdhan returned {Status}", res.StatusCode);
                return StatusCode((int)res.StatusCode, "Error fetching from AlAdhan");
            }
            var json = await res.Content.ReadAsStringAsync();
            // cache 10 minutes
            _cache.Set(cacheKey, json, TimeSpan.FromMinutes(10));
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prayer times");
            return StatusCode(500, "Internal error");
        }
    }
}
