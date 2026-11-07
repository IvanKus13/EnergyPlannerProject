namespace EnergyPlannerProject.Models;

public class Device
{
    public int Id { get; set; }

   public int UserId { get; set; }
    public User? User { get; set; }

    public int? SchemeId { get; set; }
    public Scheme? Schemes { get; set; }

    public string Name { get; set; } = string.Empty;

    public double PowerWatts { get; set; }

    public int Quantity { get; set; } = 1;

    public double HoursPerDay { get; set; }
}