using System.Text.Json.Serialization;

namespace InventoryVisibility.Samples.Models;

/// <summary>
/// Request to query on-hand inventory
/// </summary>
public class OnHandQueryRequest
{
    [JsonPropertyName("dimensionDataSource")]
    public string? DimensionDataSource { get; set; }

    [JsonPropertyName("filters")]
    public required QueryFilters Filters { get; set; }

    [JsonPropertyName("groupByValues")]
    public List<string> GroupByValues { get; set; } = new();

    [JsonPropertyName("returnNegative")]
    public bool ReturnNegative { get; set; }
}

public class QueryFilters
{
    [JsonPropertyName("OrganizationId")]
    public List<string> OrganizationId { get; set; } = new();

    [JsonPropertyName("ProductId")]
    public List<string> ProductId { get; set; } = new();

    [JsonPropertyName("SiteId")]
    public List<string> SiteId { get; set; } = new();

    [JsonPropertyName("LocationId")]
    public List<string> LocationId { get; set; } = new();

    [JsonPropertyName("LicensePlateId")]
    public List<string> LicensePlateId { get; set; } = new();
}

/// <summary>
/// Response from on-hand inventory query
/// </summary>
public class OnHandQueryResponse
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("dimensions")]
    public Dictionary<string, string> Dimensions { get; set; } = new();

    [JsonPropertyName("quantities")]
    public Dictionary<string, Dictionary<string, decimal>> Quantities { get; set; } = new();
}


