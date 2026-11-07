
namespace EnergyPlannerProject.DTO;

public class CreateSchemeDto
{
    public string Name { get; set; } = string.Empty;

    public List<CreateAccumulatorDto> Accumulators { get; set; } = new();

    public List<CreateDeviceDto> Devices { get; set; } = new();
}

public class CreateAccumulatorDto
{
    public string Name { get; set; } = string.Empty;

    public double CapacityKilowattHours { get; set; }
}

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;

    public double PowerWatts { get; set; }
}
