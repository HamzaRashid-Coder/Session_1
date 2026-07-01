namespace LabInventory.Models.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public OverallStatsDto Overall { get; set; } = new();
    public List<LabStatsDto> PerLab { get; set; } = new();
    public List<MostIssuedItemDto> MostIssuedItems { get; set; } = new();
}

public class OverallStatsDto
{
    public int TotalItems { get; set; }
    public int TotalIssued { get; set; }
    public int TotalRemaining { get; set; }
    public int TotalDefective { get; set; }
    public int TotalLost { get; set; }
    public int OverdueCount { get; set; }
}

public class LabStatsDto
{
    public int LabId { get; set; }
    public string LabName { get; set; } = "";
    public int TotalItems { get; set; }
    public int IssuedItems { get; set; }
    public int RemainingItems { get; set; }
    public int DefectiveItems { get; set; }
    public int LostItems { get; set; }
}

public class MostIssuedItemDto
{
    public string ItemName { get; set; } = "";
    public int TotalIssuances { get; set; }
}