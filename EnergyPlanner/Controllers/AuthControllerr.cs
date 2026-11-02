using EnergyPlanner.Data;
using EnergyPlannerProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("Користувач з такою поштою вже існує");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.Password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Користувач зареєстрован.");
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}