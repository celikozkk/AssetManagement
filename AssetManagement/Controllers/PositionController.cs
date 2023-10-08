using AssetManagement.Data;
using AssetManagement.Dtos;
using AssetManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    private readonly TradingContext _context;

    public PositionController(TradingContext context)
    {
        _context = context;
    }

    // GET: api/positions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Position>> GetPositionById(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position == null)
        {
            return NotFound($"Position with ID {id} not found.");
        }

        return position;
    }

    // POST: api/positions/open
    [HttpPost("open")]
    public async Task<ActionResult<Position>> OpenPosition(PositionCreateDto positionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var account = await _context.Accounts.FindAsync(positionDto.AccountId);
        if (account == null)
        {
            return NotFound("Account not found.");
        }

        var asset = await _context.Assets.FindAsync(positionDto.AssetId);
        if (asset == null) 
        {
            return NotFound("Asset not found.");
        }

        decimal totalCost = positionDto.EntryPrice * positionDto.Amount;
        if (account.Balance < totalCost)
        {
            return BadRequest("Insufficient funds.");
        }

        account.Balance -= totalCost;

        var existingPosition = await _context.Positions.FirstOrDefaultAsync(p =>
            p.AccountId == positionDto.AccountId &&
            p.AssetId == positionDto.AssetId);

        if (existingPosition != null)
        {
            existingPosition.Amount += positionDto.Amount;
        }
        else
        {
            var position = new Position
            {
                AccountId = positionDto.AccountId,
                AssetId = positionDto.AssetId,
                Amount = positionDto.Amount,
                EntryPrice = positionDto.EntryPrice,
                EntryDate = DateTime.UtcNow
            }; 
            _context.Positions.Add(position);  
        }

        await _context.SaveChangesAsync();

        return Ok();
    }
    
    // POST: api/positions/{id}/close
    [HttpPost("{id}/close")]
    public async Task<ActionResult<Position>> ClosePosition(int id, PositionCloseDto closeDto)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound();
        }

        var account = await _context.Accounts.FindAsync(position.AccountId);
        if (account == null)
        {
            return NotFound("Account not found.");
        }

        var asset = await _context.Assets.FindAsync(position.AssetId);
        if (asset == null) 
        {
            return NotFound("Asset not found.");
        }

        if (closeDto.Amount > position.Amount)
        {
            return BadRequest("You can't sell more than you own.");
        }

        decimal totalReturn = closeDto.ClosePrice * closeDto.Amount;
        account.Balance += totalReturn;

        position.Amount -= closeDto.Amount;
        if (position.Amount == 0)
        {
            _context.Positions.Remove(position);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}