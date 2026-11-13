using EnergyPlannerProject.DTO;

namespace EnergyPlanner.Services;

public static class SchemeInputValidator
{
    public const double ContinuousHoursMarker = 1000;

    public static bool IsValidDevice(CreateDeviceDto device)
    {
        if (device == null || string.IsNullOrWhiteSpace(device.Name))
            return false;

        if (!double.IsFinite(device.PowerWatts) || device.PowerWatts <= 0)
            return false;

        if (device.Quantity <= 0)
            return false;

        if (!double.IsFinite(device.HoursPerDay) || device.HoursPerDay <= 0)
            return false;

        return device.HoursPerDay <= 24 || device.HoursPerDay == ContinuousHoursMarker;
    }

    public static bool IsValidAccumulator(CreateAccumulatorDto accumulator)
    {
        if (accumulator == null || string.IsNullOrWhiteSpace(accumulator.Name))
            return false;

        if (!double.IsFinite(accumulator.CapacityKilowattHours) || accumulator.CapacityKilowattHours <= 0)
            return false;

        return IsValidEfficiency(accumulator.EfficiencyPercent);
    }

    public static bool IsValidEfficiency(double efficiencyPercent)
    {
        return double.IsFinite(efficiencyPercent)
            && efficiencyPercent > 0
            && efficiencyPercent <= 100;
    }
}
