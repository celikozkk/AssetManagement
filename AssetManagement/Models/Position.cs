namespace AssetManagement.Models;


public class Position
{
    public int Id { get; set; }
    public string AccountId { get; set; }
    public int AssetId { get; set; }
    public decimal Amount { get; set; }
    public decimal EntryPrice { get; set; }
    public DateTime EntryDate { get; set; } 
    
    
    public Account Account { get; set; }
    public Asset Asset { get; set; }
}
