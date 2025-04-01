namespace EIMS.Shared.Models;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Datasheet", "CAD Model", "Image"
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty; // For external documents like datasheets
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public int PartId { get; set; }
    public Part? Part { get; set; }
} 