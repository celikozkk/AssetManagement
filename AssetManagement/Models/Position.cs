namespace AssetManagement.Models;

public class Position
{
    public enum OrderType
    {
        Buy,
        Sell
    }

    public int Id { get; set; }
    public int AccountId { get; set; }
    public int AssetId { get; set; }
    public DateTime EntryDate { get; set; }
    public OrderType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal OrderPrice { get; set; }
    public bool IsOpen { get; set; }

    public Account Account { get; set; }
    public Asset Asset { get; set; }
}
