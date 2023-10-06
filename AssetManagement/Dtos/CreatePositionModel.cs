using System.ComponentModel.DataAnnotations;
using AssetManagement.Models;

namespace AssetManagement.Dtos;

public class CreatePositionModel
{
    [Required]
    public int AccountId { get; set; }
    
    [Required]
    public int AssetId { get; set; }

    [Required]
    public Position.OrderType Type { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount should be positive.")]
    public decimal Amount { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "OrderPrice should be positive.")]
    public decimal OrderPrice { get; set; }
}