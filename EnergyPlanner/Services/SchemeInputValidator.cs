using EnergyPlannerProject.DTO;

namespace EnergyPlanner.Services;

public static class SchemeInputValidator
{
    public const double ContinuousHoursMarker = 1000;

    public const double MaxTargetDays = 365;

    public static bool IsValidDevice(CreateDeviceDto device)
    {
        if (device == null || string.IsNullOrWhiteSpace(device.Name))
            return false;

        return IsValidDeviceValues(device.PowerWatts, device.Quantity, device.HoursPerDay);
    }

    public static bool IsValidAccumulator(CreateAccumulatorDto accumulator)
    {
        if (accumulator == null || string.IsNullOrWhiteSpace(accumulator.Name))
            return false;

        return IsValidAccumulatorValues(accumulator.CapacityKilowattHours, accumulator.EfficiencyPercent);
    }

    public static bool IsValidDeviceValues(double powerWatts, int quantity, double hoursPerDay)
    {
        return double.IsFinite(powerWatts) && powerWatts > 0
            && quantity > 0
            && double.IsFinite(hoursPerDay) && hoursPerDay > 0
            && (hoursPerDay <= 24 || hoursPerDay == ContinuousHoursMarker);
    }

    public static bool IsValidAccumulatorValues(double capacityKilowattHours, double efficiencyPercent)
    {
        return double.IsFinite(capacityKilowattHours) && capacityKilowattHours > 0
            && IsValidEfficiency(efficiencyPercent);
    }

    public static bool IsValidEfficiency(double efficiencyPercent)
    {
        return double.IsFinite(efficiencyPercent)
            && efficiencyPercent > 0
            && efficiencyPercent <= 100;
    }

    public static string? ValidateRecommendationRequest(CapacityRecommendationRequestDto? request)
    {
        if (request == null)
            return "Порожній запит.";

        if (request.Devices == null || request.Devices.Count == 0)
            return "Додайте хоча б один пристрій.";

        foreach (var device in request.Devices)
        {
            if (!IsValidDevice(device))
                return "Перевірте дані пристроїв.";
        }

        if (request.Accumulators != null)
        {
            foreach (var accumulator in request.Accumulators)
            {
                if (!IsValidAccumulator(accumulator))
                    return "Перевірте дані акумуляторів або видаліть незаповнені.";
            }
        }

        if (!double.IsFinite(request.TargetDays) || request.TargetDays <= 0 || request.TargetDays > MaxTargetDays)
            return $"Бажана автономність має бути більше 0 і не більше {MaxTargetDays} діб.";

        if (!IsValidEfficiency(request.EfficiencyPercent))
            return "КПД має бути в межах від 0 до 100%.";

        return null;
    }
}
