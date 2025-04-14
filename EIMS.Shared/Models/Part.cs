using System.ComponentModel.DataAnnotations;

namespace EIMS.Shared.Models;

public class Part
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Part name is required")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    
    public string? Footprint { get; set; }
    
    public PartType Type { get; set; }
    
    public string? LocalPartNumber { get; set; }
    
    public string? Manufacturer { get; set; }
    
    public string? ManufacturerPartNumber { get; set; }

    // Stock Information
    public decimal TotalStock { get; set; }
    public decimal OrderedStock { get; set; }
    public DateTime? LastUsed { get; set; }

    // Specifications
    public Dictionary<string, string> Dimensions { get; set; } = new();
    public Dictionary<string, string> TechnicalSpecs { get; set; } = new();
    public Dictionary<string, string> PhysicalSpecs { get; set; } = new();

    // Financial Information
    public decimal? PurchaseValue { get; set; }
    public decimal? AveragePurchasePrice { get; set; }
    public decimal? EstimatedTotalValue { get; set; }

    // Usage Information
    public List<string> UsedInProjects { get; set; } = new();
    public List<string> UsedInMetaParts { get; set; } = new();
    
    // Attachments
    public List<Document> Documents { get; set; } = new();
    public List<string> CadKeys { get; set; } = new();
    
    // Meta-part Information
    public List<Part>? Substitutes { get; set; }

    // Part Attrition Parameters
    public string? AttritionParameters { get; set; }

    // Creation Information
    public DateTime Created { get; set; } = DateTime.UtcNow;

    // Tags
    public List<string> Tags { get; set; } = new();

    // Legacy Location Information (for backward compatibility)
    public string? Location { get; set; }
    
    // New Location Information
    public int? StorageLocationId { get; set; }
    public StorageLocation? StorageLocation { get; set; }

    public string? Notes { get; set; }
} 