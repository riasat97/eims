using System.Net.Http.Json;
using EIMS.Shared.Models;

namespace EIMS.Client.Services;

// Static class to hold configuration
public static class ServiceConfig
{
    public static bool UseLocalMode { get; set; } = true;
}

public interface IPartService
{
    Task<List<Part>> GetPartsAsync();
    Task<Part?> GetPartAsync(int id);
    Task<Part?> CreatePartAsync(Part part);
    Task<bool> UpdatePartAsync(Part part);
    Task<bool> DeletePartAsync(int id);
}

public class PartService : IPartService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartService> _logger;
    
    // For local development/offline mode
    private static readonly List<Part> _localParts = new();
    private static int _nextId = 1;

    public PartService(HttpClient httpClient, ILogger<PartService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Seed with some example data if empty
        if (_localParts.Count == 0)
        {
            _localParts.Add(new Part 
            { 
                Id = _nextId++, 
                Name = "Example Resistor", 
                Description = "10K Ohm Resistor", 
                Type = PartType.Local,
                TotalStock = 25
            });
        }
    }

    public async Task<List<Part>> GetPartsAsync()
    {
        try
        {
            if (ServiceConfig.UseLocalMode)
            {
                Console.WriteLine("Using local mode for GetPartsAsync()");
                return _localParts.ToList();
            }

            try
            {
                Console.WriteLine($"Sending GET request to {_httpClient.BaseAddress}parts");
                var parts = await _httpClient.GetFromJsonAsync<List<Part>>("parts");
                Console.WriteLine($"Successfully retrieved {parts?.Count ?? 0} parts from API");
                return parts ?? new List<Part>();
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch") || ex.ToString().Contains("Content Security Policy"))
            {
                // Special case for CSP errors - fall back to local mode
                Console.WriteLine("Detected CSP error in GetPartsAsync - falling back to local mode");
                
                // No longer automatically toggle to local mode when CSP issues are detected
                // ServiceConfig.UseLocalMode = true;
                Console.WriteLine("Using fallback mode for this request only");
                
                // Return local parts
                return _localParts.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parts");
            Console.WriteLine($"Exception in GetPartsAsync: {ex.Message}");
            Console.WriteLine($"Exception details: {ex}");
            
            // Fallback to local mode if API call fails but don't toggle the global setting
            Console.WriteLine("Falling back to local data due to API error for this request only");
            return _localParts.ToList();
        }
    }

    public async Task<Part?> GetPartAsync(int id)
    {
        try
        {
            if (ServiceConfig.UseLocalMode)
            {
                return _localParts.FirstOrDefault(p => p.Id == id);
            }

            try
            {
                Console.WriteLine($"Sending GET request to {_httpClient.BaseAddress}parts/{id}");
                var part = await _httpClient.GetFromJsonAsync<Part>($"parts/{id}");
                return part;
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch") || ex.ToString().Contains("Content Security Policy"))
            {
                // Special case for CSP errors - fall back to local mode
                Console.WriteLine($"Detected CSP error in GetPartAsync({id}) - falling back to local mode");
                
                // No longer automatically toggle to local mode when CSP issues are detected
                // ServiceConfig.UseLocalMode = true;
                Console.WriteLine("Using fallback mode for this request only");
                
                // Return local part
                return _localParts.FirstOrDefault(p => p.Id == id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting part with ID {PartId}", id);
            
            // Fallback to local mode if API call fails but don't toggle the global setting
            Console.WriteLine("Exception caught - falling back to local mode for this request only");
            // ServiceConfig.UseLocalMode = true;
            return _localParts.FirstOrDefault(p => p.Id == id);
        }
    }

    public async Task<Part?> CreatePartAsync(Part part)
    {
        try
        {
            Console.WriteLine($"Creating part: {part.Name}, Type: {part.Type}");
            Console.WriteLine($"HttpClient BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine($"ServiceConfig.UseLocalMode: {ServiceConfig.UseLocalMode}");
            
            if (ServiceConfig.UseLocalMode)
            {
                // Create a new part with a unique ID
                var newPart = new Part
                {
                    Id = _nextId++,
                    Name = part.Name,
                    Description = part.Description,
                    Manufacturer = part.Manufacturer,
                    ManufacturerPartNumber = part.ManufacturerPartNumber,
                    LocalPartNumber = part.LocalPartNumber,
                    Type = part.Type,
                    Footprint = part.Footprint,
                    TotalStock = part.TotalStock,
                    OrderedStock = part.OrderedStock,
                    LastUsed = part.LastUsed,
                    Created = DateTime.UtcNow
                };
                
                // Copy collections properly
                newPart.Dimensions = part.Dimensions ?? new Dictionary<string, string>();
                newPart.TechnicalSpecs = part.TechnicalSpecs ?? new Dictionary<string, string>();
                newPart.PhysicalSpecs = part.PhysicalSpecs ?? new Dictionary<string, string>();
                newPart.UsedInProjects = part.UsedInProjects ?? new List<string>();
                newPart.UsedInMetaParts = part.UsedInMetaParts ?? new List<string>();
                newPart.Documents = part.Documents ?? new List<Document>();
                newPart.CadKeys = part.CadKeys ?? new List<string>();
                newPart.Tags = part.Tags ?? new List<string>();
                
                _localParts.Add(newPart);
                Console.WriteLine($"Created part locally with ID: {newPart.Id}");
                return newPart;
            }
            
            try
            {
                // Initialize collections to avoid null reference exceptions
                part.Dimensions ??= new Dictionary<string, string>();
                part.TechnicalSpecs ??= new Dictionary<string, string>();
                part.PhysicalSpecs ??= new Dictionary<string, string>();
                part.UsedInProjects ??= new List<string>();
                part.UsedInMetaParts ??= new List<string>();
                part.Documents ??= new List<Document>();
                part.CadKeys ??= new List<string>();
                part.Tags ??= new List<string>();
                
                Console.WriteLine($"Sending POST request to {_httpClient.BaseAddress}parts");
                var response = await _httpClient.PostAsJsonAsync("parts", part);
                
                if (response.IsSuccessStatusCode)
                {
                    var createdPart = await response.Content.ReadFromJsonAsync<Part>();
                    Console.WriteLine($"Part created successfully with ID: {createdPart?.Id}");
                    return createdPart;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create part. Status code: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                Console.WriteLine($"Error creating part: {response.StatusCode} - {errorContent}");
                
                throw new HttpRequestException($"Failed to create part: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch") || ex.ToString().Contains("Content Security Policy"))
            {
                // Special case for CSP errors - fall back to local mode
                Console.WriteLine("Detected CSP error - falling back to local mode");
                
                // No longer automatically toggle to local mode when CSP issues are detected
                // ServiceConfig.UseLocalMode = true;
                Console.WriteLine("Using fallback mode for this request only");
                
                // Create the part locally
                return await CreatePartLocallyAsync(part);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating part");
            Console.WriteLine($"Exception creating part: {ex.Message}");
            Console.WriteLine($"Exception details: {ex}");
            
            // Fallback to local mode
            Console.WriteLine("Exception caught - falling back to local mode for this request only");
            // ServiceConfig.UseLocalMode = true;
            return await CreatePartLocallyAsync(part);
        }
    }

    public async Task<bool> UpdatePartAsync(Part part)
    {
        try
        {
            if (ServiceConfig.UseLocalMode)
            {
                var existingPartIndex = _localParts.FindIndex(p => p.Id == part.Id);
                if (existingPartIndex >= 0)
                {
                    _localParts[existingPartIndex] = part;
                    return true;
                }
                return false;
            }

            try
            {
                Console.WriteLine($"Sending PUT request to {_httpClient.BaseAddress}parts/{part.Id}");
                var response = await _httpClient.PutAsJsonAsync($"parts/{part.Id}", part);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch") || ex.ToString().Contains("Content Security Policy"))
            {
                // Special case for CSP errors - fall back to local mode
                Console.WriteLine($"Detected CSP error in UpdatePartAsync({part.Id}) - falling back to local mode");
                
                // No longer automatically toggle to local mode when CSP issues are detected
                // ServiceConfig.UseLocalMode = true;
                Console.WriteLine("Using fallback mode for this request only");
                
                // Update local part
                var existingPartIndex = _localParts.FindIndex(p => p.Id == part.Id);
                if (existingPartIndex >= 0)
                {
                    _localParts[existingPartIndex] = part;
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating part with ID {PartId}", part.Id);
            
            // Fallback to local mode
            var existingPartIndex = _localParts.FindIndex(p => p.Id == part.Id);
            if (existingPartIndex >= 0)
            {
                _localParts[existingPartIndex] = part;
                return true;
            }
            
            return false;
        }
    }

    public async Task<bool> DeletePartAsync(int id)
    {
        try
        {
            if (ServiceConfig.UseLocalMode)
            {
                var existingPartIndex = _localParts.FindIndex(p => p.Id == id);
                if (existingPartIndex >= 0)
                {
                    _localParts.RemoveAt(existingPartIndex);
                    return true;
                }
                return false;
            }

            try
            {
                Console.WriteLine($"Sending DELETE request to {_httpClient.BaseAddress}parts/{id}");
                var response = await _httpClient.DeleteAsync($"parts/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch") || ex.ToString().Contains("Content Security Policy"))
            {
                // Special case for CSP errors - fall back to local mode
                Console.WriteLine($"Detected CSP error in DeletePartAsync({id}) - falling back to local mode");
                
                // No longer automatically toggle to local mode when CSP issues are detected
                // ServiceConfig.UseLocalMode = true;
                Console.WriteLine("Using fallback mode for this request only");
                
                // Delete local part
                var existingPartIndex = _localParts.FindIndex(p => p.Id == id);
                if (existingPartIndex >= 0)
                {
                    _localParts.RemoveAt(existingPartIndex);
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting part with ID {PartId}", id);
            
            // Fallback to local mode
            var existingPartIndex = _localParts.FindIndex(p => p.Id == id);
            if (existingPartIndex >= 0)
            {
                _localParts.RemoveAt(existingPartIndex);
                return true;
            }
            
            return false;
        }
    }
    
    private Task<Part?> CreatePartLocallyAsync(Part part)
    {
        // Create a new part with a unique ID
        var newPart = new Part
        {
            Id = _nextId++,
            Name = part.Name,
            Description = part.Description,
            Manufacturer = part.Manufacturer,
            ManufacturerPartNumber = part.ManufacturerPartNumber,
            LocalPartNumber = part.LocalPartNumber,
            Type = part.Type,
            Footprint = part.Footprint,
            TotalStock = part.TotalStock,
            OrderedStock = part.OrderedStock,
            LastUsed = part.LastUsed,
            Created = DateTime.UtcNow
        };
        
        // Copy collections properly
        newPart.Dimensions = part.Dimensions ?? new Dictionary<string, string>();
        newPart.TechnicalSpecs = part.TechnicalSpecs ?? new Dictionary<string, string>();
        newPart.PhysicalSpecs = part.PhysicalSpecs ?? new Dictionary<string, string>();
        newPart.UsedInProjects = part.UsedInProjects ?? new List<string>();
        newPart.UsedInMetaParts = part.UsedInMetaParts ?? new List<string>();
        newPart.Documents = part.Documents ?? new List<Document>();
        newPart.CadKeys = part.CadKeys ?? new List<string>();
        newPart.Tags = part.Tags ?? new List<string>();
        
        _localParts.Add(newPart);
        Console.WriteLine($"Created part locally with ID: {newPart.Id}");
        return Task.FromResult<Part?>(newPart);
    }
} 