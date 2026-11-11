namespace EnergyPlannerProject.Models;

public class Scheme
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Accumulator> Accumulators { get; set; }
        = new List<Accumulator>();

    public ICollection<Device> Devices { get; set; }
        = new List<Device>();
}