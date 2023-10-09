using System.Security.Claims;
using AssetManagement.Data;
using AssetManagement.Dtos;
using AssetManagement.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers;

[Authorize]
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
    
    // GET: api/positions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositionsByAccountId()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return NotFound("Account not found.");
        }

        var positions = await _context.Positions.Where(p => p.AccountId == account.Id).ToListAsync();

        return Ok(_mapper.Map<List<PositionDto>>(positions));
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
        
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (position.AccountId != accountId)
        {
            return Unauthorized("You do not have permission to access this position.");
        }

        return Ok(_mapper.Map<PositionDto>(position));
    }

    // POST: api/positions/open
    [HttpPost("open")]
    public async Task<ActionResult<PositionDto>> OpenPosition(PositionCreateDto positionCreateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return NotFound($"Account not found. {accountId}");
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

        var existingPosition = await _context.Positions.FirstOrDefaultAsync(p =>
            p.AccountId == accountId &&
            p.AssetId == positionCreateDto.AssetId);

        Position returnedPosition;
        
        if (existingPosition != null)
        {
            // updating existing position
            if (existingPosition.AccountId != accountId)
            {
                return Unauthorized("You do not have permission to update this position.");
            }
            existingPosition.Amount += positionCreateDto.Amount;
            returnedPosition = existingPosition;
        }
        else
        {
            // opening new position
            var position = new Position
            {
                AccountId = accountId,
                AssetId = positionCreateDto.AssetId,
                Amount = positionCreateDto.Amount,
                EntryPrice = asset.LastPrice,
                EntryDate = DateTime.UtcNow
            };
            _context.Positions.Add(position);
            returnedPosition = position;
        }

        account.Balance -= totalCost;
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<PositionDto>(returnedPosition));
    }
    
    // todo: add ClosePositionDto read DTO to display sell price
    // POST: api/positions/{id}/close
    [HttpPost("{id}/close")]
    public async Task<ActionResult<PositionDto>> ClosePosition(int id, PositionCloseDto closeDto)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound("Position not found.");
        }

        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (position.AccountId != accountId)
        {
            return Unauthorized("You do not have permission to close this position.");
        }
        
        var account = await _context.Accounts.FindAsync(accountId);
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