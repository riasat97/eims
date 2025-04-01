namespace EIMS.Shared.Models;

public class OctopartResponse
{
    public OctopartData Data { get; set; } = new();
    public OctopartExtensions Extensions { get; set; } = new();
}

public class OctopartData
{
    public OctopartSupSearch SupSearch { get; set; } = new();
}

public class OctopartSupSearch
{
    public int Hits { get; set; }
    public List<OctopartResult> Results { get; set; } = new();
}

public class OctopartResult
{
    public OctopartPart Part { get; set; } = new();
}

public class OctopartPart
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mpn { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public List<OctopartDescription> Descriptions { get; set; } = new();
    public OctopartPrice? MedianPrice1000 { get; set; }
    public OctopartCategory? Category { get; set; }
    public OctopartManufacturer Manufacturer { get; set; } = new();
    public List<OctopartSpecification> Specs { get; set; } = new();
    public OctopartImage? BestImage { get; set; }
    public OctopartDatasheet? BestDatasheet { get; set; }
}

public class OctopartPrice
{
    public int Quantity { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class OctopartCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class OctopartManufacturer
{
    public string Name { get; set; } = string.Empty;
    public string HomepageUrl { get; set; } = string.Empty;
}

public class OctopartExtensions
{
    public string RequestId { get; set; } = string.Empty;
}

// For convenience, we'll map the Octopart results to this simplified model to use in our UI
public class OctopartSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Mpn { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string ManufacturerUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DatasheetUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, string>> Specs { get; set; } = new();
}

public class OctopartSpec
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
}

public class OctopartDescription
{
    public string Text { get; set; } = string.Empty;
}

public class OctopartSpecification
{
    public OctopartAttribute Attribute { get; set; } = new();
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public string Units { get; set; } = string.Empty;
}

public class OctopartAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}

public class OctopartValue
{
    public string Display { get; set; } = string.Empty;
}

public class OctopartImage
{
    public string Url { get; set; } = string.Empty;
    public string CreditString { get; set; } = string.Empty;
    public string CreditUrl { get; set; } = string.Empty;
}

public class OctopartDatasheet
{
    public string Url { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public string CreditString { get; set; } = string.Empty;
} 