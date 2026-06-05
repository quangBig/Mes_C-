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
public class MachineController : ControllerBase
{
    private readonly MESDbContext _context;

    public MachineController(MESDbContext context) => _context = context;

    // GET: api/Machine/statuses  — dùng để render select box ở frontend
    [HttpGet("statuses")]
    public ActionResult<IEnumerable<MachineStatusOption>> GetStatuses()
        => Ok(MachineStatus.AllOptions);

    // GET: api/Machine
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineResponseDto>>> GetAll()
    {
        var machines = await _context.Machines
            .Include(m => m.ProductionLine)
            .Select(m => new MachineResponseDto
            {
                MachineId   = m.MachineId,
                LineId      = m.LineId,
                LineName    = m.ProductionLine != null ? m.ProductionLine.LineName : string.Empty,
                MachineCode = m.MachineCode,
                MachineName = m.MachineName,
                Status      = m.Status
            })
            .ToListAsync();

        return Ok(machines);
    }

    // GET: api/Machine/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MachineResponseDto>> Get(int id)
    {
        var machine = await _context.Machines
            .Include(m => m.ProductionLine)
            .FirstOrDefaultAsync(m => m.MachineId == id);

        if (machine == null) return NotFound();

        var dto = new MachineResponseDto
        {
            MachineId   = machine.MachineId,
            LineId      = machine.LineId,
            LineName    = machine.ProductionLine?.LineName ?? string.Empty,
            MachineCode = machine.MachineCode,
            MachineName = machine.MachineName,
            Status      = machine.Status
        };

        return Ok(dto);
    }

    // POST: api/Machine
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<MachineResponseDto>> Create([FromBody] CreateMachineDto dto)
    {
        var lineExists = await _context.ProductionLines.AnyAsync(l => l.LineId == dto.LineId);
        if (!lineExists)
            return BadRequest($"ProductionLine with Id={dto.LineId} does not exist.");

        var machine = new Machine
        {
            LineId      = dto.LineId,
            MachineCode = dto.MachineCode,
            MachineName = dto.MachineName,
            Status      = dto.Status
        };

        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();

        var result = new MachineResponseDto
        {
            MachineId   = machine.MachineId,
            LineId      = machine.LineId,
            MachineCode = machine.MachineCode,
            MachineName = machine.MachineName,
            Status      = machine.Status
        };

        return CreatedAtAction(nameof(Get), new { id = machine.MachineId }, result);
    }

    // PUT: api/Machine/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMachineDto dto)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound();

        var lineExists = await _context.ProductionLines.AnyAsync(l => l.LineId == dto.LineId);
        if (!lineExists)
            return BadRequest($"ProductionLine with Id={dto.LineId} does not exist.");

        machine.LineId      = dto.LineId;
        machine.MachineCode = dto.MachineCode;
        machine.MachineName = dto.MachineName;
        machine.Status      = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Machine/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound();

        _context.Machines.Remove(machine);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
