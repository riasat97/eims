using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EIMS.Shared.Models;
using EIMS.Shared.Services;

namespace EIMS.Client.Services;

public class OctopartService : IOctopartService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _accessToken;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OctopartService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        
        // Get and validate configuration
        _clientId = configuration["Octopart:ClientId"] ?? string.Empty;
        _clientSecret = configuration["Octopart:ClientSecret"] ?? string.Empty;
        
        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
        {
            throw new InvalidOperationException(
                $"Octopart API credentials not properly configured. ClientId: '{_clientId?.Length}' chars, ClientSecret: '{_clientSecret?.Length}' chars");
        }
        
        Console.WriteLine($"OctopartService initialized with ClientId: '{_clientId}'");
        
        _httpClient.BaseAddress = new Uri("https://api.nexar.com/graphql");
    }

    private async Task EnsureAccessToken()
    {
        if (!string.IsNullOrEmpty(_accessToken))
            return;

        Console.WriteLine("Attempting to get OAuth token using form-encoded approach...");
        
        try
        {
            string tokenEndpoint = "https://identity.nexar.com/connect/token";
            using var httpClient = new HttpClient();
            
            // Use content-type application/x-www-form-urlencoded with client_id and client_secret in the body
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("scope", "supply.domain")
            });

            // Set Accept header
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            Console.WriteLine($"Requesting token for client ID: {_clientId}");
            
            // Send the request directly as form content
            var response = await httpClient.PostAsync(tokenEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Token response status: {response.StatusCode}");
            Console.WriteLine($"Response content: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (responseContent.Contains("invalid_client"))
                {
                    Console.WriteLine("Received invalid_client error - will try direct string content");
                    await TryDirectStringContent();
                    return;
                }
                
                throw new InvalidOperationException($"Failed to obtain access token: {response.StatusCode} - {responseContent}");
            }

            // Parse the token response
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonOptions);
            _accessToken = tokenResponse?.access_token ?? throw new InvalidOperationException("Null access token received");
            
            // Set the authorization header for future requests
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            Console.WriteLine("Successfully obtained access token");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting access token: {ex.Message}");
            Console.WriteLine("Trying fallback method...");
            await TryDirectStringContent();
        }
    }
    
    private async Task TryDirectStringContent()
    {
        Console.WriteLine("Attempting direct string content approach for OAuth...");
        
        try
        {
            string tokenEndpoint = "https://identity.nexar.com/connect/token";
            using var httpClient = new HttpClient();
            
            // Format request body as a plain string
            string requestBody = $"grant_type=client_credentials&client_id={Uri.EscapeDataString(_clientId)}&client_secret={Uri.EscapeDataString(_clientSecret)}&scope=supply.domain";
            
            // Create StringContent with correct content type
            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            
            // Don't add any extra headers - use a clean request
            
            Console.WriteLine("Sending token request with direct string content...");
            var response = await httpClient.PostAsync(tokenEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Direct token response status: {response.StatusCode}");
            Console.WriteLine($"Response content: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"All token attempts failed: {response.StatusCode} - {responseContent}");
            }

            // Parse the token response
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonOptions);
            _accessToken = tokenResponse?.access_token ?? throw new InvalidOperationException("Null access token received");
            
            // Set the authorization header for future requests
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            Console.WriteLine("Successfully obtained access token with direct string approach");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in direct string approach: {ex.Message}");
            throw;
        }
    }

    // Token response class with JsonPropertyName attributes matching API response
    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string access_token { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int expires_in { get; set; }

        [JsonPropertyName("token_type")]
        public string token_type { get; set; } = string.Empty;
        
        [JsonPropertyName("scope")]
        public string scope { get; set; } = string.Empty;
    }

    public Task<string> GetBase64Image(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return Task.FromResult(string.Empty);
            }

            Console.WriteLine($"Loading image from {imageUrl}");
            
            // Return direct URL with fixed domain
            if (imageUrl.Contains("sigma.octopart.com") || imageUrl.Contains("octopart.com"))
            {
                return Task.FromResult(imageUrl);
            }
            
            // Return placeholder for non-octopart images
            return Task.FromResult("data:image/png;base64,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");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
            return Task.FromResult(string.Empty);
        }
    }

    private class ImageResponse
    {
        public ImageData? Data { get; set; }
    }

    private class ImageData
    {
        public SupImage? SupImage { get; set; }
    }

    private class SupImage
    {
        public string? Url { get; set; }
        public string? CreditString { get; set; }
        public string? CreditUrl { get; set; }
    }

    public async Task<List<OctopartSearchResult>> SearchByMpn(string mpn)
    {
        try
        {
            await EnsureAccessToken();
            
            var query = @"
            query partSearch($mpn: String!) {
              supSearch(q: $mpn, limit: 5) {
                hits
                results {
                  part {
                    id
                    name
                    mpn
                    shortDescription
                    descriptions {
                      text
                    }
                    medianPrice1000 {
                      price
                      currency
                    }
                    category {
                      id
                      name
                    }
                    manufacturer {
                      name
                      homepageUrl
                    }
                    specs {
                      attribute {
                        name
                        group
                      }
                      value
                      displayValue
                      units
                    }
                    bestImage {
                      url
                      creditString
                      creditUrl
                    }
                    bestDatasheet {
                      url
                      pageCount
                      creditString
                    }
                  }
                }
              }
            }";

            var variables = new { mpn };
            var graphqlRequest = new { query, variables };
            
            var content = new StringContent(
                JsonSerializer.Serialize(graphqlRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"GraphQL query failed: {response.StatusCode}");
                Console.Error.WriteLine($"Error response: {errorContent}");
                return new List<OctopartSearchResult>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG - GraphQL Response: {responseContent}");

            var octopartResponse = JsonSerializer.Deserialize<OctopartResponse>(responseContent, _jsonOptions);
            
            return await MapToSearchResults(octopartResponse);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error searching Octopart by MPN: {ex.Message}");
            return new List<OctopartSearchResult>();
        }
    }

    public async Task<List<OctopartSearchResult>> Search(string query)
    {
        try
        {
            await EnsureAccessToken();
            
            var graphqlQuery = @"
            query partSearch($searchQuery: String!) {
              supSearch(q: $searchQuery, limit: 5) {
                hits
                results {
                  part {
                    id
                    name
                    mpn
                    shortDescription
                    descriptions {
                      text
                    }
                    medianPrice1000 {
                      price
                      currency
                    }
                    category {
                      id
                      name
                    }
                    manufacturer {
                      name
                      homepageUrl
                    }
                    specs {
                      attribute {
                        name
                        group
                      }
                      value
                      displayValue
                      units
                    }
                    bestImage {
                      url
                      creditString
                      creditUrl
                    }
                    bestDatasheet {
                      url
                      pageCount
                      creditString
                    }
                  }
                }
              }
            }";

            var variables = new { searchQuery = query };
            var graphqlRequest = new { query = graphqlQuery, variables };
            
            var content = new StringContent(
                JsonSerializer.Serialize(graphqlRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"GraphQL query failed: {response.StatusCode}");
                Console.Error.WriteLine($"Error response: {errorContent}");
                return new List<OctopartSearchResult>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG - GraphQL Response: {responseContent}");

            var octopartResponse = JsonSerializer.Deserialize<OctopartResponse>(responseContent, _jsonOptions);
            
            return await MapToSearchResults(octopartResponse);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error searching Octopart: {ex.Message}");
            return new List<OctopartSearchResult>();
        }
    }

    private async Task<List<OctopartSearchResult>> MapToSearchResults(OctopartResponse? response)
    {
        if (response?.Data?.SupSearch?.Results == null)
            return new List<OctopartSearchResult>();

        var results = new List<OctopartSearchResult>();
        foreach (var r in response.Data.SupSearch.Results)
        {
            var imageUrl = r.Part.BestImage?.Url ?? "";
            var base64Image = string.Empty;
            
            if (!string.IsNullOrEmpty(imageUrl))
            {
                base64Image = await GetBase64Image(imageUrl);
            }
            
            var datasheetUrl = r.Part.BestDatasheet?.Url ?? "";
            
            Console.WriteLine($"DEBUG - Part: {r.Part.Mpn}");
            Console.WriteLine($"DEBUG - Original Image URL: {imageUrl}");
            Console.WriteLine($"DEBUG - Base64 Image Length: {base64Image.Length}");
            Console.WriteLine($"DEBUG - Image Credit: {r.Part.BestImage?.CreditString}");
            Console.WriteLine($"DEBUG - Datasheet URL: {datasheetUrl}");
            
            results.Add(new OctopartSearchResult
            {
                Id = r.Part.Id,
                Mpn = r.Part.Mpn,
                Name = r.Part.Name,
                Description = r.Part.ShortDescription ?? "",
                Manufacturer = r.Part.Manufacturer?.Name ?? "",
                ManufacturerUrl = r.Part.Manufacturer?.HomepageUrl ?? "",
                Category = r.Part.Category?.Name ?? "",
                DatasheetUrl = datasheetUrl,
                ImageUrl = base64Image,
                Specs = MapSpecifications(r.Part.Specs)
            });
        }
            
        Console.WriteLine($"Mapped {results.Count} search results");
        return results;
    }

    private Dictionary<string, Dictionary<string, string>> MapSpecifications(List<OctopartSpecification> specs)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        
        foreach (var spec in specs)
        {
            var category = !string.IsNullOrEmpty(spec.Attribute.Group) 
                ? spec.Attribute.Group 
                : CategorizeSpec(spec.Attribute.Name);

            if (!result.ContainsKey(category))
            {
                result[category] = new Dictionary<string, string>();
            }

            var value = spec.DisplayValue;
            if (!string.IsNullOrEmpty(spec.Units))
            {
                value += $" {spec.Units}";
            }
            else if (!string.IsNullOrEmpty(spec.Value))
            {
                value = spec.Value;
            }

            result[category][spec.Attribute.Name] = value;
        }
        
        return result;
    }

    private string CategorizeSpec(string specName)
    {
        return specName.ToLower() switch
        {
            var s when s.Contains("length") || s.Contains("width") || s.Contains("height") || s.Contains("diameter") => "Dimensions",
            var s when s.Contains("voltage") || s.Contains("current") || s.Contains("resistance") || s.Contains("frequency") => "Technical",
            var s when s.Contains("color") || s.Contains("material") || s.Contains("weight") || s.Contains("finish") => "Physical",
            _ => "Other"
        };
    }
} 