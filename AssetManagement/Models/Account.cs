using Microsoft.AspNetCore.Identity;

namespace AssetManagement.Models;

public class Account : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public decimal Balance { get; set; } = 10000m;
    public float NotificationRate { get; set; } 
}