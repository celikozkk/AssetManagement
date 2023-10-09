using AssetManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Data;

public class TradingContext : IdentityDbContext<Account>
{
    public TradingContext(DbContextOptions<TradingContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Position> Positions { get; set; }
}