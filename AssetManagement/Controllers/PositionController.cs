using AssetManagement.Data;
using AssetManagement.Dtos;
using AssetManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    private readonly TradingContext _context;
    private readonly PositionService _positionService;

    public PositionController(TradingContext context, PositionService positionService)
    {
        _context = context;
        _positionService = positionService;
    }

    // GET: api/positions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position == null)
        {
            return NotFound($"Position with ID {id} not found.");
        }

        return Ok(position);
    }

    
    // POST: api/positions
    [HttpPost]
    public async Task<IActionResult> OpenPosition([FromBody] CreatePositionModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var newPosition = await _positionService.OpenPositionAsync(model);

        if (newPosition == null)
        {
            return BadRequest("Unable to open position. Check logs for details.");
        }
        
        return CreatedAtAction(nameof(GetPosition), new { id = newPosition.Id }, newPosition);
    }
    
    // PUT: api/positions/{id}/close
    [HttpPut("{id}/close")]
    public async Task<IActionResult> ClosePosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        
        if (position == null)
        {
            return NotFound();
        }
        
        if (!position.IsOpen)
        {
            return BadRequest("Position is already closed.");
        }

        position.IsOpen = false;
        await _context.SaveChangesAsync();
        return Ok(position);
    }
}