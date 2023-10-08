using AssetManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly TradingContext _context;

    public AssetController(TradingContext context)
    {
        _context = context;
    }

    // GET: api/assets
    [HttpGet]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await _context.Assets.ToListAsync();
        return Ok(assets);
    }

    // GET: api/assets/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null)
        {
            return NotFound("Asset not found.");
        }
        
        return Ok(asset);
    }
}