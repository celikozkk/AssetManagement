namespace AssetManagement.Models;

public class Asset
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public decimal LastPrice { get; set; }
}