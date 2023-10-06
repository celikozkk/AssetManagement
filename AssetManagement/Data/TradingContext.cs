using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Data;

public class TradingContext : DbContext
{
    public TradingContext(DbContextOptions<TradingContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Position> Positions { get; set; }
}