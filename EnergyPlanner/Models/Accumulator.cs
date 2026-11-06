namespace EnergyPlannerProject.Models;

public class Accumulator
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;

    public double CapacityKilowattHours { get; set; }

    public double EfficiencyPercent { get; set; }
}