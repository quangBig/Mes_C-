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
public class ProductController : ControllerBase
{
    private readonly MESDbContext _context;

    public ProductController(MESDbContext context) => _context = context;

    // GET: api/Product
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _context.Products
            .Select(p => new ProductResponseDto
            {
                ProductId   = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/Product/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> Get(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        return Ok(new ProductResponseDto
        {
            ProductId   = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName
        });
    }

    // POST: api/Product
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.ProductCode == dto.ProductCode);
        if (exists)
            return BadRequest($"ProductCode '{dto.ProductCode}' đã tồn tại.");

        var product = new Product
        {
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = new ProductResponseDto
        {
            ProductId   = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName
        };

        return CreatedAtAction(nameof(Get), new { id = product.ProductId }, result);
    }

    // PUT: api/Product/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        var codeExists = await _context.Products
            .AnyAsync(p => p.ProductCode == dto.ProductCode && p.ProductId != id);
        if (codeExists)
            return BadRequest($"ProductCode '{dto.ProductCode}' đã tồn tại.");

        product.ProductCode = dto.ProductCode;
        product.ProductName = dto.ProductName;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Product/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
