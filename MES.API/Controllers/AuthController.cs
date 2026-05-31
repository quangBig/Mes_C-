using System.Text.RegularExpressions;
using MES.API.Data;
using MES.API.DTOs;
using MES.API.Models;
using MES.API.Services;
using Microsoft.AspNetCore.Authorization;
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

    // ===== ĐĂNG NHẬP =====
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

        if (user == null)
            return Unauthorized(new { message = "Tài khoản không tồn tại" });

        if (string.IsNullOrEmpty(user.PasswordHash))
            return Unauthorized(new { message = "Tài khoản chưa được thiết lập mật khẩu hợp lệ" });

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized(new { message = "Mật khẩu không đúng" });

        var token = _jwtService.GenerateToken(user);
        return Ok(new
        {
            token,
            user.UserName,
            Role = user.Role!.RoleName
        });
    }

    // ===== LẤY TẤT CẢ USER =====
    [Authorize(Roles = "Admin")]
    [HttpGet("getAllUser")]
    public async Task<ActionResult> GetAllUser()
    {
        var users = await _context.Users.Include(u => u.Role).ToListAsync();
        return Ok(users);
    }

    // ===== TẠO USER MỚI =====
    [Authorize(Roles = "Admin")]
    [HttpPost("User")]
    public async Task<ActionResult> CreateUser(CreateUserDto dto)
    {
        // Validate password
        var passwordError = ValidatePassword(dto.Password);
        if (passwordError != null)
            return BadRequest(new { message = passwordError });

        // Kiểm tra user/email đã tồn tại chưa
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email || u.UserName == dto.UserName);

        if (existingUser != null)
            return BadRequest(new { message = "Username hoặc Email đã tồn tại" });

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Tạo người dùng thành công" });
    }

    // ===== CẬP NHẬT USER =====
    [Authorize(Roles = "Admin")]
    [HttpPut("User")]
    public async Task<ActionResult> UpdateUser(UpdateUserDto dto)
    {
        // Validate password
        var passwordError = ValidatePassword(dto.Password);
        if (passwordError != null)
            return BadRequest(new { message = passwordError });

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == dto.Id);

        if (user == null)
            return NotFound(new { message = "Người dùng không tồn tại" });

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.RoleId = dto.RoleId;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật người dùng thành công" });
    }

    // ===== XÓA USER =====
    [Authorize(Roles = "Admin")]
    [HttpDelete("User/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Người dùng không tồn tại" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Xóa người dùng thành công" });
    }

    // ===== HELPER: Validate password =====
    /// <summary>
    /// Kiểm tra độ mạnh password:
    /// - Ít nhất 7 ký tự (> 6)
    /// - Ít nhất 1 chữ hoa (A-Z)
    /// - Ít nhất 1 chữ số (0-9)
    /// - Ít nhất 1 ký tự đặc biệt
    /// </summary>
    private static string? ValidatePassword(string password)
    {
        if (password.Length <= 6)
            return "Mật khẩu phải có hơn 6 ký tự";

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return "Mật khẩu phải có ít nhất 1 chữ cái in hoa (A-Z)";

        if (!Regex.IsMatch(password, @"[0-9]"))
            return "Mật khẩu phải có ít nhất 1 chữ số (0-9)";

        if (!Regex.IsMatch(password, @"[!@#$%^&*()\-_=+\[\]{};':""\\|,.<>/?`~]"))
            return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt (!@#$%^&*...)";

        return null; // Hợp lệ
    }
}
