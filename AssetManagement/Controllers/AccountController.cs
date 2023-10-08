using AssetManagement.Data;
using AssetManagement.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController : ControllerBase
{
    private readonly TradingContext _context;
    private readonly IMapper _mapper;

    public AccountController(TradingContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/accounts/{id}/positions
    [HttpGet("{id}/positions")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositionsByAccountId(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound("Account not found.");
        }

        var positions = await _context.Positions.Where(p => p.AccountId == account.Id).ToListAsync();

        return Ok(_mapper.Map<List<PositionDto>>(positions));
    }
}