namespace AssetManagement.Dtos;

public class PositionDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public decimal Amount { get; set; }
    public decimal EntryPrice { get; set; }
    public DateTime EntryDate { get; set; } 
}