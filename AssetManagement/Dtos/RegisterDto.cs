using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Dtos;

public class RegisterDto
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}