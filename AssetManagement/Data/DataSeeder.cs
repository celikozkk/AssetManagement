using AssetManagement.Models;

namespace AssetManagement.Data;

public class DataSeeder
{
    private readonly TradingContext _context;

    public DataSeeder(TradingContext context)
    {
        _context = context;
    }

    public void SeedData()
    {
        if (!_context.Assets.Any()) // Check if the Assets table is empty
        {
            var assets = new List<Asset>
            {
                new Asset { Symbol = "BTC", LastPrice = 0m },
                new Asset { Symbol = "ETH", LastPrice = 0m },
                new Asset { Symbol = "AVAX", LastPrice = 0m },
            };

            _context.Assets.AddRange(assets);
            _context.SaveChanges();
        }
    }
}
