namespace AssetManagement.Models;

public class Account
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public decimal Balance { get; set; } = 10000m;
    public float NotificationRate { get; set; } 
}