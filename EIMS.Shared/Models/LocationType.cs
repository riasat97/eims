namespace EIMS.Shared.Models;

/// <summary>
/// Defines the type of storage location generation method
/// </summary>
public enum LocationType
{
    /// <summary>
    /// A single manually named location
    /// </summary>
    Single,
    
    /// <summary>
    /// A one-dimensional range of locations (e.g., a row)
    /// </summary>
    Row,
    
    /// <summary>
    /// A two-dimensional grid of locations (e.g., a matrix)
    /// </summary>
    Grid,
    
    /// <summary>
    /// A three-dimensional grid of locations (e.g., a cube)
    /// </summary>
    ThreeDGrid
} 