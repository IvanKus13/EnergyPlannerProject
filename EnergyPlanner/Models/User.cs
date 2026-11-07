namespace EnergyPlannerProject.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Scheme> Schemes { get; set; }
        = new List<Scheme>();

    public ICollection<Accumulator> Accumulators { get; set; }
        = new List<Accumulator>();

    public ICollection<Device> Devices { get; set; }
        = new List<Device>();
}