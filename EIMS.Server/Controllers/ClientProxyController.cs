using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace EIMS.Server.Controllers
{
    [ApiController]
    [Route("client-api")]
    public class ClientProxyController : ControllerBase
    {
        private readonly ILogger<ClientProxyController> _logger;
        
        public ClientProxyController(ILogger<ClientProxyController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            try
            {
                // This is a pass-through to the actual parts controller
                // It's on the same domain as the client, so no CSP issues
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:5063/api/parts");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                
                return StatusCode((int)response.StatusCode, "Error accessing API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to parts API");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpGet("parts/{id}")]
        public async Task<IActionResult> GetPart(int id)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"http://localhost:5063/api/parts/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                
                return StatusCode((int)response.StatusCode, "Error accessing API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to parts API");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost("parts")]
        public async Task<IActionResult> CreatePart()
        {
            try
            {
                // Read the body content
                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                // Forward the request to the actual API
                using var client = new HttpClient();
                var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5063/api/parts", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Content(responseContent, "application/json");
                }
                
                return StatusCode((int)response.StatusCode, "Error accessing API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to parts API");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPut("parts/{id}")]
        public async Task<IActionResult> UpdatePart(int id)
        {
            try
            {
                // Read the body content
                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                // Forward the request to the actual API
                using var client = new HttpClient();
                var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"http://localhost:5063/api/parts/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                
                return StatusCode((int)response.StatusCode, "Error accessing API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to parts API");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete("parts/{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.DeleteAsync($"http://localhost:5063/api/parts/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                
                return StatusCode((int)response.StatusCode, "Error accessing API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to parts API");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 