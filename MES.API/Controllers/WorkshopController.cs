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
public class WorkshopController : ControllerBase
{
    private readonly MESDbContext _context;

    public WorkshopController(MESDbContext context)
    {
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workshop>>> GetAll()
    {
        var workshops = await _context.Workshops.Include(w => w.Factory).ToListAsync();
        return Ok(workshops);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Workshop>> GetById(int id)
    {
        var workshop = await _context.Workshops.Include(w => w.Factory).FirstOrDefaultAsync(w => w.WorkshopId == id);
        if (workshop == null)
            return NotFound(new { message = "Không tìm thấy phân xưởng" });
        return Ok(workshop);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create(CreateWorkshopDto dto)
    {
        if (await _context.Workshops.AnyAsync(w => w.WorkshopCode == dto.WorkshopCode))
        {
            return BadRequest(new { message = "Mã phân xưởng đã tồn tại" });
        }

        var workshop = new Workshop
        {
            WorkshopCode = dto.WorkshopCode,
            WorkshopName = dto.WorkshopName,
            FactoryId = dto.FactoryId
        };

        _context.Workshops.Add(workshop);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tạo phân xưởng thành công", data = workshop });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateWorkshopDto dto)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null)
            return NotFound(new { message = "Không tìm thấy phân xưởng" });

        if (await _context.Workshops.AnyAsync(w => w.WorkshopCode == dto.WorkshopCode && w.WorkshopId != id))
        {
            return BadRequest(new { message = "Mã phân xưởng đã tồn tại" });
        }

        workshop.WorkshopCode = dto.WorkshopCode;
        workshop.WorkshopName = dto.WorkshopName;
        workshop.FactoryId = dto.FactoryId;

        _context.Workshops.Update(workshop);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật phân xưởng thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null)
            return NotFound(new { message = "Không tìm thấy phân xưởng" });

        _context.Workshops.Remove(workshop);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Xóa phân xưởng thành công" });
    }

}
