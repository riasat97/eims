using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EIMS.Shared.Models;
using EIMS.Server.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace EIMS.Server.Services;

/// <summary>
/// Service for generating storage locations based on the specified pattern
/// </summary>
public interface ILocationGenerationService
{
    Task<LocationGenerationPreview> GeneratePreviewAsync(LocationGenerationRequest request);
    Task<bool> GenerateLocationsAsync(LocationGenerationRequest request);
}

public class LocationGenerationService : ILocationGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocationGenerationService> _logger;
    private const int RecommendedLimit = 500;
    private const int SampleSize = 10;

    public LocationGenerationService(ApplicationDbContext context, ILogger<LocationGenerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Generate a preview of the locations that would be created based on the generation request
    /// </summary>
    /// <param name="request">The location generation request parameters</param>
    /// <returns>A preview of the locations that would be generated</returns>
    public async Task<LocationGenerationPreview> GeneratePreviewAsync(LocationGenerationRequest request)
    {
        try
        {
            // Generate all location names based on request
            var locationNames = GenerateLocationNames(request);
            
            // Create a grid view for 2D and 3D types
            var gridView = CreateGridView(request, locationNames);
            
            // Create the preview object
            var preview = new LocationGenerationPreview
            {
                Request = request,
                LocationNames = locationNames,
                GridView = gridView
            };
            
            // Add a warning if there are too many locations
            if (locationNames.Count > RecommendedLimit)
            {
                preview.WarningMessage = $"This will generate {locationNames.Count} locations, which exceeds our recommended limit of {RecommendedLimit}. This may take some time and consume significant resources.";
            }
            
            return preview;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating location preview");
            throw;
        }
    }
    
    /// <summary>
    /// Generate storage locations from the request
    /// </summary>
    /// <param name="request">The location generation request parameters</param>
    /// <returns>True if locations were created successfully</returns>
    public async Task<bool> GenerateLocationsAsync(LocationGenerationRequest request)
    {
        try
        {
            // Generate all location names
            var locationNames = GenerateLocationNames(request);
            
            // Check for existing locations with the same names
            var existingNames = await _context.StorageLocations
                .Where(l => locationNames.Contains(l.Name))
                .Select(l => l.Name)
                .ToListAsync();
                
            if (existingNames.Any())
            {
                _logger.LogWarning("Location generation failed - existing names: {Names}", 
                    string.Join(", ", existingNames));
                return false;
            }
            
            // Create storage location objects
            var now = DateTime.UtcNow;
            var locations = locationNames.Select(name => new StorageLocation
            {
                Name = name,
                IsSinglePartOnly = request.IsSinglePartOnly,
                Created = now,
                LastModified = now,
                Metadata = new Dictionary<string, string>
                {
                    { "GeneratedBy", "LocationGenerationService" },
                    { "GeneratedOn", now.ToString("o") }
                }
            }).ToList();
            
            // Save to database
            await _context.StorageLocations.AddRangeAsync(locations);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully generated {Count} storage locations", locations.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating locations");
            return false;
        }
    }
    
    #region Helper Methods
    
    /// <summary>
    /// Create a grid view for 2D and 3D grid visualizations
    /// </summary>
    private List<List<string>> CreateGridView(LocationGenerationRequest request, List<string> names)
    {
        var gridView = new List<List<string>>();
        
        // Only create grid views for Grid and ThreeDGrid types
        if (request.Type == LocationType.Single || request.Type == LocationType.Row)
        {
            return gridView;
        }
        
        // For 2D grid
        if (request.Type == LocationType.Grid)
        {
            int rowCount = GetRangeSize(request.Range1UsesLetters, request.Range1StartLetter, request.Range1EndLetter, 
                                      request.Range1StartNumber, request.Range1EndNumber);
            
            int colCount = GetRangeSize(request.Range2UsesLetters, request.Range2StartLetter, request.Range2EndLetter, 
                                      request.Range2StartNumber, request.Range2EndNumber);
            
            for (int i = 0; i < rowCount; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < colCount; j++)
                {
                    int index = i * colCount + j;
                    if (index < names.Count)
                    {
                        row.Add(names[index]);
                    }
                }
                gridView.Add(row);
            }
        }
        
        // For 3D grid (simplified - showing only 2D layers)
        if (request.Type == LocationType.ThreeDGrid)
        {
            int rowCount = GetRangeSize(request.Range1UsesLetters, request.Range1StartLetter, request.Range1EndLetter, 
                                      request.Range1StartNumber, request.Range1EndNumber);
            
            int colCount = GetRangeSize(request.Range2UsesLetters, request.Range2StartLetter, request.Range2EndLetter, 
                                      request.Range2StartNumber, request.Range2EndNumber);
                                      
            int depthCount = GetRangeSize(request.Range3UsesLetters, request.Range3StartLetter, request.Range3EndLetter, 
                                      request.Range3StartNumber, request.Range3EndNumber);
            
            for (int depth = 0; depth < depthCount; depth++)
            {
                // Add a separator row
                if (depth > 0)
                {
                    gridView.Add(new List<string> { $"--- Level {depth} ---" });
                }
                
                for (int i = 0; i < rowCount; i++)
                {
                    var row = new List<string>();
                    for (int j = 0; j < colCount; j++)
                    {
                        int index = (depth * rowCount * colCount) + (i * colCount) + j;
                        if (index < names.Count)
                        {
                            row.Add(names[index]);
                        }
                    }
                    gridView.Add(row);
                }
            }
        }
        
        return gridView;
    }
    
    /// <summary>
    /// Get the size of a range based on the start and end values
    /// </summary>
    private int GetRangeSize(bool usesLetters, char? startLetter, char? endLetter, int? startNumber, int? endNumber)
    {
        if (usesLetters && startLetter.HasValue && endLetter.HasValue)
        {
            return Math.Abs(endLetter.Value - startLetter.Value) + 1;
        }
        else if (!usesLetters && startNumber.HasValue && endNumber.HasValue)
        {
            return Math.Abs(endNumber.Value - startNumber.Value) + 1;
        }
        
        return 1; // Default if invalid values
    }
    
    /// <summary>
    /// Generate location names based on the request type
    /// </summary>
    private List<string> GenerateLocationNames(LocationGenerationRequest request)
    {
        return request.Type switch
        {
            LocationType.Single => GenerateSingleName(request),
            LocationType.Row => Generate1DNames(request),
            LocationType.Grid => Generate2DNames(request),
            LocationType.ThreeDGrid => Generate3DNames(request),
            _ => throw new ArgumentException("Invalid location type")
        };
    }
    
    /// <summary>
    /// Generate a single location name
    /// </summary>
    private List<string> GenerateSingleName(LocationGenerationRequest request)
    {
        return new List<string> { request.Prefix };
    }
    
    /// <summary>
    /// Generate names for a 1D range (row)
    /// </summary>
    private List<string> Generate1DNames(LocationGenerationRequest request)
    {
        var names = new List<string>();
        
        if (request.Range1UsesLetters)
        {
            if (!request.Range1StartLetter.HasValue || !request.Range1EndLetter.HasValue)
            {
                return names;
            }
            
            char start = char.ToUpper(request.Range1StartLetter.Value);
            char end = char.ToUpper(request.Range1EndLetter.Value);
            
            // Validate the characters are letters
            if (!char.IsLetter(start) || !char.IsLetter(end))
            {
                return names;
            }
            
            // Ensure end is greater than or equal to start
            if (end < start)
            {
                (start, end) = (end, start); // Swap values
            }
            
            // Ensure we stay within A-Z range
            start = (char)Math.Max(start, 'A');
            end = (char)Math.Min(end, 'Z');
            
            for (char c = start; c <= end; c++)
            {
                char displayChar = request.Range1Capitalize ? char.ToUpper(c) : char.ToLower(c);
                names.Add($"{request.Prefix}{displayChar}");
            }
        }
        else
        {
            if (!request.Range1StartNumber.HasValue || !request.Range1EndNumber.HasValue)
            {
                return names;
            }
            
            int start = request.Range1StartNumber.Value;
            int end = request.Range1EndNumber.Value;
            
            // Ensure end is greater than or equal to start
            if (end < start)
            {
                (start, end) = (end, start); // Swap values
            }
            
            int padding = 0;
            if (request.Range1PadWithZeros)
            {
                padding = end.ToString().Length;
            }
            
            for (int i = start; i <= end; i++)
            {
                names.Add($"{request.Prefix}{i.ToString().PadLeft(padding, '0')}");
            }
        }
        
        return names;
    }
    
    /// <summary>
    /// Generate names for a 2D grid
    /// </summary>
    private List<string> Generate2DNames(LocationGenerationRequest request)
    {
        var names = new List<string>();
        
        // Generate values for first dimension (rows)
        var rowValues = GenerateDimensionValues(
            request.Range1UsesLetters,
            request.Range1StartLetter,
            request.Range1EndLetter,
            request.Range1StartNumber,
            request.Range1EndNumber,
            request.Range1Capitalize,
            request.Range1PadWithZeros);
            
        // Generate values for second dimension (columns)
        var colValues = GenerateDimensionValues(
            request.Range2UsesLetters,
            request.Range2StartLetter,
            request.Range2EndLetter,
            request.Range2StartNumber,
            request.Range2EndNumber,
            request.Range2Capitalize,
            request.Range2PadWithZeros);
            
        // Create combinations
        foreach (var row in rowValues)
        {
            foreach (var col in colValues)
            {
                names.Add($"{request.Prefix}{row}{request.Separator1}{col}");
            }
        }
        
        return names;
    }
    
    /// <summary>
    /// Generate names for a 3D grid
    /// </summary>
    private List<string> Generate3DNames(LocationGenerationRequest request)
    {
        var names = new List<string>();
        
        // Generate values for first dimension (rows)
        var rowValues = GenerateDimensionValues(
            request.Range1UsesLetters,
            request.Range1StartLetter,
            request.Range1EndLetter,
            request.Range1StartNumber,
            request.Range1EndNumber,
            request.Range1Capitalize,
            request.Range1PadWithZeros);
            
        // Generate values for second dimension (columns)
        var colValues = GenerateDimensionValues(
            request.Range2UsesLetters,
            request.Range2StartLetter,
            request.Range2EndLetter,
            request.Range2StartNumber,
            request.Range2EndNumber,
            request.Range2Capitalize,
            request.Range2PadWithZeros);
            
        // Generate values for third dimension (depth)
        var depthValues = GenerateDimensionValues(
            request.Range3UsesLetters,
            request.Range3StartLetter,
            request.Range3EndLetter,
            request.Range3StartNumber,
            request.Range3EndNumber,
            request.Range3Capitalize,
            request.Range3PadWithZeros);
            
        // Create combinations
        foreach (var depth in depthValues)
        {
            foreach (var row in rowValues)
            {
                foreach (var col in colValues)
                {
                    names.Add($"{request.Prefix}{row}{request.Separator1}{col}{request.Separator2}{depth}");
                }
            }
        }
        
        return names;
    }
    
    /// <summary>
    /// Generate values for a dimension (letters or numbers)
    /// </summary>
    private List<string> GenerateDimensionValues(
        bool usesLetters, 
        char? startLetter, 
        char? endLetter, 
        int? startNumber, 
        int? endNumber,
        bool capitalize,
        bool padWithZeros)
    {
        var values = new List<string>();
        
        if (usesLetters)
        {
            if (!startLetter.HasValue || !endLetter.HasValue)
            {
                return values;
            }
            
            // Keep original case
            char start = startLetter.Value;
            char end = endLetter.Value;
            
            // Force the same case as the start letter
            bool useUppercase = char.IsUpper(start);
            if (useUppercase != char.IsUpper(end))
            {
                end = useUppercase ? char.ToUpper(end) : char.ToLower(end);
            }
            
            // For range comparison, use uppercase
            char upperStart = char.ToUpper(start);
            char upperEnd = char.ToUpper(end);
            
            // Validate the characters are letters
            if (!char.IsLetter(upperStart) || !char.IsLetter(upperEnd))
            {
                return values;
            }
            
            // Ensure end is greater than or equal to start (using uppercase comparison)
            if (upperEnd < upperStart)
            {
                (upperStart, upperEnd) = (upperEnd, upperStart); // Swap values
                (start, end) = (end, start); // Also swap original values
            }
            
            // Ensure we stay within A-Z range
            if (upperStart < 'A') upperStart = 'A';
            if (upperEnd > 'Z') upperEnd = 'Z';
            
            // Use the case from start letter
            bool startIsUpper = char.IsUpper(start);
            
            // Generate letters in the proper case
            for (char c = upperStart; c <= upperEnd; c++)
            {
                // If capitalize is true, always use uppercase regardless of start letter case
                // Otherwise, maintain the same case as the start letter
                values.Add(capitalize ? char.ToUpper(c).ToString() : 
                          (startIsUpper ? char.ToUpper(c).ToString() : char.ToLower(c).ToString()));
            }
        }
        else
        {
            if (!startNumber.HasValue || !endNumber.HasValue)
            {
                return values;
            }
            
            int start = startNumber.Value;
            int end = endNumber.Value;
            
            // Ensure positive numbers
            start = Math.Max(0, start);
            end = Math.Max(0, end);
            
            // Ensure end is greater than or equal to start
            if (end < start)
            {
                (start, end) = (end, start); // Swap values
            }
            
            int padding = 0;
            if (padWithZeros)
            {
                padding = end.ToString().Length;
            }
            
            for (int i = start; i <= end; i++)
            {
                values.Add(i.ToString().PadLeft(padding, '0'));
            }
        }
        
        return values;
    }
    
    #endregion
} 