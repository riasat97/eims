namespace EIMS.Shared.Models;

/// <summary>
/// Represents a preview of locations that will be generated based on a generation request
/// </summary>
public class LocationGenerationPreview
{
    /// <summary>
    /// The original request that this preview is based on
    /// </summary>
    public LocationGenerationRequest Request { get; set; } = null!;
    
    /// <summary>
    /// The list of location names that would be generated
    /// </summary>
    public List<string> LocationNames { get; set; } = new();
    
    /// <summary>
    /// For 2D and 3D grids, the names organized in a matrix for display
    /// </summary>
    public List<List<string>> GridView { get; set; } = new();
    
    /// <summary>
    /// Total number of locations that would be generated
    /// </summary>
    public int TotalLocations => LocationNames.Count;
    
    /// <summary>
    /// Warning message if a large number of locations would be generated
    /// </summary>
    public string? WarningMessage { get; set; }
    
    /// <summary>
    /// Flag indicating if there is a warning about the number of locations
    /// </summary>
    public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
} 