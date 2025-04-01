using Microsoft.AspNetCore.Mvc;

namespace EIMS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProxyController> _logger;

    public ProxyController(ILogger<ProxyController> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

    [HttpGet("image")]
    public async Task<IActionResult> GetImage([FromQuery] string url)
    {
        try
        {
            _logger.LogInformation($"Proxying image request for URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch image. Status: {response.StatusCode}");
                return NotFound();
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            
            _logger.LogInformation($"Successfully proxied image. Size: {imageBytes.Length} bytes");
            
            return File(imageBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying image");
            return StatusCode(500);
        }
    }
} 