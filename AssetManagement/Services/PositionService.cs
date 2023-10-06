using AssetManagement.Data;
using AssetManagement.Dtos;
using AssetManagement.Models;

namespace AssetManagement.Services;

public class PositionService
{
    private readonly TradingContext _context;
    private readonly ILogger<PositionService> _logger;

    public PositionService(TradingContext context, ILogger<PositionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Position?> OpenPositionAsync(CreatePositionModel model)
    {
        var account = await _context.Accounts.FindAsync(model.AccountId);
        if (account == null)
        {
            _logger.LogWarning("Account with id {AccountId} not found.", model.AccountId);
            return null;   
        }
        
        var asset = await _context.Assets.FindAsync(model.AssetId);
        if (asset == null)
        {
            _logger.LogWarning("Asset with id {AssetId} not found.", model.AssetId);
            return null;
        }

        // no balance changing operations for the account because the position is still open
        if (model.Type == Position.OrderType.Buy && account.Balance < model.Amount * model.OrderPrice)
        {
            _logger.LogWarning("Insufficient funds to open position for AccountId: {AccountId}", model.AccountId);
            return null;
        }

        Position newPosition = new Position
        {
            AccountId = model.AccountId,
            AssetId = model.AssetId,
            EntryDate = DateTime.UtcNow,
            Type = model.Type,
            Amount = model.Amount,
            OrderPrice = model.OrderPrice,
            IsOpen = true
        };

        _context.Positions.Add(newPosition);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Position opened for AccountId: {AccountId} with AssetId: {AssetId}", model.AccountId, model.AssetId);
        
        return newPosition;
    }
}
