namespace MES.API.DTOs;

public class LoginDto
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

public class CreateUserDto
{
    public required string UserName { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public int RoleId { get; set; }
}

public class UpdateUserDto
{
    public int Id { get; set; }

    public required string UserName { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public int RoleId { get; set; }
}