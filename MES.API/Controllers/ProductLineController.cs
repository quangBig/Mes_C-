using MES.API.Data;
using MES.API.DTOs;
using MES.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MES.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductLineController : ControllerBase
{
    private readonly MESDbContext _context;

    public ProductLineController(MESDbContext context) => _context = context;

    // GET: api/productline
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductionLineResponseDto>>> GetAll()
    {
        var lines = await _context.ProductionLines
            .Include(l => l.Workshop)
            .Select(l => new ProductionLineResponseDto
            {
                LineId       = l.LineId,
                LineCode     = l.LineCode,
                LineName     = l.LineName,
                WorkshopId   = l.WorkshopId,
                WorkshopName = l.Workshop != null ? l.Workshop.WorkshopName : null
            })
            .ToListAsync();

        return Ok(lines);
    }

    // GET: api/productline/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionLineResponseDto>> Get(int id)
    {
        var line = await _context.ProductionLines
            .Include(l => l.Workshop)
            .FirstOrDefaultAsync(l => l.LineId == id);

        if (line == null) return NotFound();

        var dto = new ProductionLineResponseDto
        {
            LineId       = line.LineId,
            LineCode     = line.LineCode,
            LineName     = line.LineName,
            WorkshopId   = line.WorkshopId,
            WorkshopName = line.Workshop?.WorkshopName
        };

        return Ok(dto);
    }

    // POST: api/productline
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductionLineResponseDto>> Create([FromBody] CreateProductionLineDto dto)
    {
        var workshopExists = await _context.Workshops.AnyAsync(w => w.WorkshopId == dto.WorkshopId);
        if (!workshopExists)
            return BadRequest($"Workshop with Id={dto.WorkshopId} does not exist.");

        var line = new ProductionLine
        {
            WorkshopId = dto.WorkshopId,
            LineCode   = dto.LineCode,
            LineName   = dto.LineName
        };

        _context.ProductionLines.Add(line);
        await _context.SaveChangesAsync();

        var result = new ProductionLineResponseDto
        {
            LineId     = line.LineId,
            LineCode   = line.LineCode,
            LineName   = line.LineName,
            WorkshopId = line.WorkshopId
        };

        return CreatedAtAction(nameof(Get), new { id = line.LineId }, result);
    }

    // PUT: api/productline/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionLineDto dto)
    {
        var line = await _context.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();

        var workshopExists = await _context.Workshops.AnyAsync(w => w.WorkshopId == dto.WorkshopId);
        if (!workshopExists)
            return BadRequest($"Workshop with Id={dto.WorkshopId} does not exist.");

        line.WorkshopId = dto.WorkshopId;
        line.LineCode   = dto.LineCode;
        line.LineName   = dto.LineName;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/productline/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var line = await _context.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();

        _context.ProductionLines.Remove(line);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
