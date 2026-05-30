using MES.API.Data;
using MES.API.DTOs;
using MES.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MES.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MESDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(MESDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(
            u => u.UserName == dto.UserName
        );
        if (user == null)
        {
            return Unauthorized(
                new
                {
                    message = "User not found"
                }
                );
        }
        if (user.PasswordHash != dto.Password)
        {
            return Unauthorized(
                new
                {
                    message = "Invalid password"
                }
                );
        }
        var token = _jwtService.GenerateToken(user);
        return Ok(
            new
            {
                token,
                user.UserName,
                user.Role
            }
            );
    }
}
