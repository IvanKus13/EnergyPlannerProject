using EnergyPlanner.Data;
using EnergyPlannerProject.Models;
using EnergyPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController : ControllerBase
{
    private const string InvalidCredentialsMessage = "Неправильна пошта або пароль";
    private const string DuplicateEmailMessage = "Користувач з такою поштою вже існує";

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
        var email = AuthValidator.NormalizeEmail(request.Email);

        var error = AuthValidator.ValidateEmail(email)
            ?? AuthValidator.ValidatePassword(request.Password);

        if (error != null)
            return BadRequest(new { message = error });

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return BadRequest(new { message = DuplicateEmailMessage });

        var user = new User { Email = email };
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.Entry(user).State = EntityState.Detached;

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest(new { message = DuplicateEmailMessage });

            throw;
        }

        return Ok(new { message = "Користувача зареєстровано." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = AuthValidator.NormalizeEmail(request.Email);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request.Password))
            return Unauthorized(new { message = InvalidCredentialsMessage });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized(new { message = InvalidCredentialsMessage });

        var result = _passwordService.VerifyPassword(user, request.Password);

        if (result == PasswordCheckResult.Failed)
            return Unauthorized(new { message = InvalidCredentialsMessage });

        if (result == PasswordCheckResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordService.HashPassword(user, request.Password);
            await _context.SaveChangesAsync();

            LogPasswordRehashed(_logger, user.Id);
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Пароль користувача {UserId} переведено на хеш.")]
    private static partial void LogPasswordRehashed(ILogger logger, int userId);

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