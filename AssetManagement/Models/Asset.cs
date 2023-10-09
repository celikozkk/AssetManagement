using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Models;

public class Asset
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    
    [Column(TypeName = "decimal(18, 4)")]
    public decimal LastPrice { get; set; }
}