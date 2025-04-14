using System.ComponentModel.DataAnnotations;

namespace EIMS.Shared.Models;

/// <summary>
/// Represents a physical storage location where parts can be stored
/// </summary>
public class StorageLocation
{
    /// <summary>
    /// Unique identifier for the storage location
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The name/identifier of the storage location (e.g., "A1", "Box1-B3")
    /// </summary>
    [Required(ErrorMessage = "Location name is required")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of the storage location
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The type of location (Single, Row, Grid, 3D Grid)
    /// </summary>
    public LocationType Type { get; set; }
    
    /// <summary>
    /// Flag indicating if this location can only store a single part type
    /// </summary>
    public bool IsSinglePartOnly { get; set; }
    
    /// <summary>
    /// Parent location ID if this is a child location (null if it's a top-level location)
    /// </summary>
    public int? ParentLocationId { get; set; }
    
    /// <summary>
    /// Reference to the parent location (null if it's a top-level location)
    /// </summary>
    public StorageLocation? ParentLocation { get; set; }
    
    /// <summary>
    /// Child locations contained within this location
    /// </summary>
    public List<StorageLocation> ChildLocations { get; set; } = new();
    
    /// <summary>
    /// Parts stored at this location
    /// </summary>
    public List<Part> StoredParts { get; set; } = new();
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata about the location (e.g., temperature requirements, etc.)
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
} 