
namespace EnergyPlannerProject.DTO;

public class CalculationResultDto
{
    public bool ContinuousMode { get; set; }
    public bool FullDayAvailable { get; set; }
    public double DailyConsumption { get; set; }
    public double RuntimeHours { get; set; }
    public double RuntimeDays { get; set; }
    public int AdditionalFullDays { get; set; }
    public double NextDayPercent { get; set; }
}
