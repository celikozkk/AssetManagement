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
}