using EnergyPlanner.Services;
using EnergyPlannerProject.DTO;
using EnergyPlannerProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnergyPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationsController : ControllerBase
{
    private readonly EnergyCalculationService _calculationService;

    public CalculationsController(EnergyCalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    [HttpPost("capacity-recommendation")]
    public IActionResult RecommendCapacity([FromBody] CapacityRecommendationRequestDto request)
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Потрібно увійти в акаунт." });

        var error = SchemeInputValidator.ValidateRecommendationRequest(request);

        if (error != null)
            return BadRequest(new { message = error });

        var devices = new List<Device>();

        foreach (var item in request.Devices)
        {
            devices.Add(new Device
            {
                Name = item.Name.Trim(),
                PowerWatts = item.PowerWatts,
                Quantity = item.Quantity,
                HoursPerDay = item.HoursPerDay
            });
        }

        var accumulators = new List<Accumulator>();

        foreach (var item in request.Accumulators ?? new List<CreateAccumulatorDto>())
        {
            accumulators.Add(new Accumulator
            {
                Name = item.Name.Trim(),
                CapacityKilowattHours = item.CapacityKilowattHours,
                EfficiencyPercent = item.EfficiencyPercent
            });
        }

        var result = _calculationService.RecommendCapacity(
            devices,
            accumulators,
            request.TargetDays,
            request.EfficiencyPercent);

        return Ok(result);
    }
}
