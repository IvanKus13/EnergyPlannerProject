using EnergyPlanner.Data;
using EnergyPlannerProject.Models;
using EnergyPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string InvalidCredentialsMessage = "Неправильна пошта або пароль";

    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext context,
        PasswordService passwordService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Користувач з такою поштою вже існує");

        var user = new User { Email = request.Email };
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Користувача зареєстровано.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Unauthorized(InvalidCredentialsMessage);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(InvalidCredentialsMessage);

        var result = _passwordService.VerifyPassword(user, request.Password);

        if (result == PasswordCheckResult.Failed)
            return Unauthorized(InvalidCredentialsMessage);

        if (result == PasswordCheckResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordService.HashPassword(user, request.Password);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Пароль користувача {UserId} переведено на хеш.", user.Id);
        }

        HttpContext.Session.SetString("UserEmail", user.Email);

        return Ok(new { email = user.Email });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (email == null)
            return Unauthorized();

        return Ok(new { email });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}