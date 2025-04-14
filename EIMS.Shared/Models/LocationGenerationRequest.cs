namespace EIMS.Shared.Models;

/// <summary>
/// Represents a request to generate one or more storage locations
/// </summary>
public class LocationGenerationRequest
{
    /// <summary>
    /// The type of location generation (Single, Row, Grid, 3D Grid)
    /// </summary>
    public LocationType Type { get; set; }
    
    /// <summary>
    /// Common prefix for all generated locations (e.g., "box1-")
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Flag to indicate if generated locations can only store a single part type
    /// </summary>
    public bool IsSinglePartOnly { get; set; }
    
    // Configuration for Range 1 (rows)
    /// <summary>
    /// Whether Range 1 uses letters (true) or numbers (false)
    /// </summary>
    public bool Range1UsesLetters { get; set; } = true;
    
    /// <summary>
    /// Starting letter for Range 1 if using letters
    /// </summary>
    public char? Range1StartLetter { get; set; }
    
    /// <summary>
    /// Ending letter for Range 1 if using letters
    /// </summary>
    public char? Range1EndLetter { get; set; }
    
    /// <summary>
    /// Starting number for Range 1 if using numbers
    /// </summary>
    public int? Range1StartNumber { get; set; }
    
    /// <summary>
    /// Ending number for Range 1 if using numbers
    /// </summary>
    public int? Range1EndNumber { get; set; }
    
    /// <summary>
    /// Flag to capitalize letters in Range 1
    /// </summary>
    public bool Range1Capitalize { get; set; }
    
    /// <summary>
    /// Flag to pad numbers with leading zeros in Range 1
    /// </summary>
    public bool Range1PadWithZeros { get; set; }
    
    /// <summary>
    /// Separator between Range 1 and Range 2
    /// </summary>
    public string Separator1 { get; set; } = string.Empty;
    
    // Configuration for Range 2 (columns)
    /// <summary>
    /// Whether Range 2 uses letters (true) or numbers (false)
    /// </summary>
    public bool Range2UsesLetters { get; set; } = false;
    
    /// <summary>
    /// Starting letter for Range 2 if using letters
    /// </summary>
    public char? Range2StartLetter { get; set; }
    
    /// <summary>
    /// Ending letter for Range 2 if using letters
    /// </summary>
    public char? Range2EndLetter { get; set; }
    
    /// <summary>
    /// Starting number for Range 2 if using numbers
    /// </summary>
    public int? Range2StartNumber { get; set; }
    
    /// <summary>
    /// Ending number for Range 2 if using numbers
    /// </summary>
    public int? Range2EndNumber { get; set; }
    
    /// <summary>
    /// Flag to capitalize letters in Range 2
    /// </summary>
    public bool Range2Capitalize { get; set; }
    
    /// <summary>
    /// Flag to pad numbers with leading zeros in Range 2
    /// </summary>
    public bool Range2PadWithZeros { get; set; }
    
    /// <summary>
    /// Separator between Range 2 and Range 3
    /// </summary>
    public string Separator2 { get; set; } = string.Empty;
    
    // Configuration for Range 3 (depth - for 3D grids)
    /// <summary>
    /// Whether Range 3 uses letters (true) or numbers (false)
    /// </summary>
    public bool Range3UsesLetters { get; set; } = false;
    
    /// <summary>
    /// Starting letter for Range 3 if using letters
    /// </summary>
    public char? Range3StartLetter { get; set; }
    
    /// <summary>
    /// Ending letter for Range 3 if using letters
    /// </summary>
    public char? Range3EndLetter { get; set; }
    
    /// <summary>
    /// Starting number for Range 3 if using numbers
    /// </summary>
    public int? Range3StartNumber { get; set; }
    
    /// <summary>
    /// Ending number for Range 3 if using numbers
    /// </summary>
    public int? Range3EndNumber { get; set; }
    
    /// <summary>
    /// Flag to capitalize letters in Range 3
    /// </summary>
    public bool Range3Capitalize { get; set; }
    
    /// <summary>
    /// Flag to pad numbers with leading zeros in Range 3
    /// </summary>
    public bool Range3PadWithZeros { get; set; }
} 