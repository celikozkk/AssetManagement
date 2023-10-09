using System.ComponentModel.DataAnnotations;
using AssetManagement.Models;

namespace AssetManagement.Dtos;

public class PositionCreateDto
{
    [Required]
    public int AssetId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount should be positive.")]
    public decimal Amount { get; set; }
}