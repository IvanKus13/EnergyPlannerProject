using EnergyPlannerProject.Models;

namespace EnergyPlanner.Services
{

    public class EnergyCalculationService
    {
        public double CalculateRuntime(
    IEnumerable<Accumulator> accumulators,
    IEnumerable<Device> devices)
        {
            double totalCapacity = accumulators.Sum(a =>
                a.CapacityKilowattHours *
                (a.EfficiencyPercent / 100));

            double totalPower = devices.Sum(d =>
                d.PowerWatts * d.Quantity);

            if (totalCapacity <= 0)
                throw new ArgumentException("Battery capacity must be greater than zero.");

            if (totalPower <= 0)
                throw new ArgumentException("Total device power must be greater than zero.");

            double availableEnergy = totalCapacity * 1000;

            return availableEnergy / totalPower;
        }
    }

}
