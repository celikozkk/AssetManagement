using AssetManagement.Data;
using AssetManagement.Dtos;
using AssetManagement.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    private readonly TradingContext _context;
    private readonly IMapper _mapper;

    public PositionController(TradingContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/positions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PositionDto>> GetPositionById(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position == null)
        {
            return NotFound($"Position with ID {id} not found.");
        }

        return Ok(_mapper.Map<PositionDto>(position));
    }

    // POST: api/positions/open
    [HttpPost("open")]
    public async Task<ActionResult<PositionDto>> OpenPosition(PositionCreateDto positionCreateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var account = await _context.Accounts.FindAsync(positionCreateDto.AccountId);
        if (account == null)
        {
            return NotFound("Account not found.");
        }

        var asset = await _context.Assets.FindAsync(positionCreateDto.AssetId);
        if (asset == null) 
        {
            return NotFound("Asset not found.");
        }

        decimal totalCost = asset.LastPrice * positionCreateDto.Amount;
        if (account.Balance < totalCost)
        {
            return BadRequest("Insufficient funds.");
        }

        account.Balance -= totalCost;

        var existingPosition = await _context.Positions.FirstOrDefaultAsync(p =>
            p.AccountId == positionCreateDto.AccountId &&
            p.AssetId == positionCreateDto.AssetId);

        Position returnedPosition;
        
        if (existingPosition != null)
        {
            // existing position updated
            existingPosition.Amount += positionCreateDto.Amount;
            returnedPosition = existingPosition;
        }
        else
        {
            // new position opened
            var position = new Position
            {
                AccountId = positionCreateDto.AccountId,
                AssetId = positionCreateDto.AssetId,
                Amount = positionCreateDto.Amount,
                EntryPrice = asset.LastPrice,
                EntryDate = DateTime.UtcNow
            };
            _context.Positions.Add(position);
            returnedPosition = position;
        }

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<PositionDto>(returnedPosition));
    }
    
    // POST: api/positions/{id}/close
    [HttpPost("{id}/close")]
    public async Task<ActionResult<PositionDto>> ClosePosition(int id, PositionCloseDto closeDto)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound("Position not found.");
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

        decimal totalReturn = asset.LastPrice * closeDto.Amount;
        account.Balance += totalReturn;

        position.Amount -= closeDto.Amount;
        if (position.Amount == 0)
        {
            _context.Positions.Remove(position);
        }

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<PositionDto>(position));
    }
}