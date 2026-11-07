using EnergyPlanner.Data;
using EnergyPlannerProject.DTO;
using EnergyPlannerProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchemesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SchemesController(AppDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public async Task<IActionResult> CreateScheme(
        [FromBody] CreateSchemeDto request)
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Потрібно увійти в акаунт." });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized(new { message = "Користувача не знайдено." });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Введіть назву схеми." });

        if (request.Accumulators == null || request.Accumulators.Count == 0)
            return BadRequest(new { message = "Додайте хоча б один акумулятор." });

        if (request.Devices == null || request.Devices.Count == 0)
            return BadRequest(new { message = "Додайте хоча б один пристрій." });

        if (request.Accumulators.Any(a =>
            string.IsNullOrWhiteSpace(a.Name) ||
            !double.IsFinite(a.CapacityKilowattHours) ||
            a.CapacityKilowattHours <= 0))
        {
            return BadRequest(new { message = "Перевірте дані акумуляторів." });
        }

        if (request.Devices.Any(d =>
            string.IsNullOrWhiteSpace(d.Name) ||
            !double.IsFinite(d.PowerWatts) ||
            d.PowerWatts <= 0))
        {
            return BadRequest(new { message = "Перевірте дані пристроїв." });
        }

        var scheme = new Scheme
        {
            Name = request.Name.Trim(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Accumulators)
        {
            scheme.Accumulators.Add(new Accumulator
            {
                Name = item.Name.Trim(),
                CapacityKilowattHours = item.CapacityKilowattHours,
                EfficiencyPercent = 100,
                UserId = user.Id
            });
        }

        foreach (var item in request.Devices)
        {
            scheme.Devices.Add(new Device
            {
                Name = item.Name.Trim(),
                PowerWatts = item.PowerWatts,
                Quantity = 1,
                HoursPerDay = 0,
                UserId = user.Id
            });
        }

        _context.Schemes.Add(scheme);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Схему успішно збережено.",
            schemeId = scheme.Id
        });
    }


    [HttpGet]
    public async Task<IActionResult> GetSchemes()
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new
            {
                message = "Потрібно авторизуватися."
            });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized();
        }

        var schemes = await _context.Schemes
            .Where(s => s.UserId == user.Id)
            .Include(s => s.Accumulators)
            .Include(s => s.Devices)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                createdAt = s.CreatedAt,

                accumulators = s.Accumulators.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    capacityKilowattHours = a.CapacityKilowattHours
                }),

                devices = s.Devices.Select(d => new
                {
                    id = d.Id,
                    name = d.Name,
                    powerWatts = d.PowerWatts
                })
            })
            .ToListAsync();

        return Ok(schemes);
    }
}