namespace EnergyPlannerProject.DTO;

public class CapacityRecommendationRequestDto
{
    public List<CreateDeviceDto> Devices { get; set; } = new();

    public List<CreateAccumulatorDto> Accumulators { get; set; } = new();

    public double TargetDays { get; set; }

    public double EfficiencyPercent { get; set; } = 90;
}

public class CapacityRecommendationDto
{
    public double TargetDays { get; set; }
    public double EfficiencyPercent { get; set; }

    public double DailyConsumption { get; set; }
    public double RequiredEnergy { get; set; }
    public double RequiredCapacityKilowattHours { get; set; }

    public double CurrentAvailableEnergy { get; set; }
    public double AdditionalCapacityKilowattHours { get; set; }
    public bool IsCurrentCapacityEnough { get; set; }
}
