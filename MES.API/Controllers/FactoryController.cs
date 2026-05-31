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
public class FactoryController : ControllerBase
{
    private readonly MESDbContext _context;

    public FactoryController(MESDbContext context)
    {
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Factory>>> GetAll()
    {
        var factories = await _context.Factories.ToListAsync();
        return Ok(factories);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Factory>> GetById(int id)
    {
        var factory = await _context.Factories.FindAsync(id);
        if (factory == null)
            return NotFound(new { message = "Không tìm thấy nhà máy" });
        return Ok(factory);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create(CreateFactoryDto dto)
    {
        if (await _context.Factories.AnyAsync(f => f.FactoryCode == dto.FactoryCode))
        {
            return BadRequest(new { message = "Mã nhà máy đã tồn tại" });
        }

        var factory = new Factory
        {
            FactoryCode = dto.FactoryCode,
            FactoryName = dto.FactoryName
        };

        _context.Factories.Add(factory);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tạo nhà máy thành công", data = factory });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateFactoryDto dto)
    {
        var factory = await _context.Factories.FindAsync(id);
        if (factory == null)
            return NotFound(new { message = "Không tìm thấy nhà máy" });

        if (await _context.Factories.AnyAsync(f => f.FactoryCode == dto.FactoryCode && f.FactoryId != id))
        {
            return BadRequest(new { message = "Mã nhà máy đã tồn tại" });
        }

        factory.FactoryCode = dto.FactoryCode;
        factory.FactoryName = dto.FactoryName;

        _context.Factories.Update(factory);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật nhà máy thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var factory = await _context.Factories.FindAsync(id);
        if (factory == null)
            return NotFound(new { message = "Không tìm thấy nhà máy" });

        _context.Factories.Remove(factory);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Xóa nhà máy thành công" });
    }
}
