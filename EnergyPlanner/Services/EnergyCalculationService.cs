
using EnergyPlannerProject.DTO;
using EnergyPlannerProject.Models;

namespace EnergyPlanner.Services
{
    public class EnergyCalculationService
    {
        public CalculationResultDto CalculateRuntime(
            IEnumerable<Accumulator> accumulators,
            IEnumerable<Device> devices)
        {
            var accumulatorList = accumulators.ToList();
            var deviceList = devices.ToList();

            double availableEnergy = accumulatorList.Sum(a =>
                a.CapacityKilowattHours * 1000 *
                (a.EfficiencyPercent / 100));

            if (availableEnergy <= 0 || deviceList.Count == 0)
                throw new ArgumentException("Invalid energy values.");

            if (deviceList.Any(d =>
                d.PowerWatts <= 0 ||
                d.Quantity <= 0 ||
                d.HoursPerDay <= 0 ||
                (d.HoursPerDay > 24 && d.HoursPerDay != SchemeInputValidator.ContinuousHoursMarker)))
            {
                throw new ArgumentException("Invalid device values.");
            }

            bool continuousMode = deviceList.All(d =>
                d.HoursPerDay == SchemeInputValidator.ContinuousHoursMarker);

            double totalPower = deviceList.Sum(d =>
                d.PowerWatts * d.Quantity);

            if (continuousMode)
            {
                double runtimeHours = availableEnergy / totalPower;

                return new CalculationResultDto
                {
                    ContinuousMode = true,
                    RuntimeHours = runtimeHours
                };
            }

            double dailyConsumption = deviceList.Sum(d =>
                d.PowerWatts * d.Quantity *
                (d.HoursPerDay == SchemeInputValidator.ContinuousHoursMarker ? 24 : d.HoursPerDay));

            if (dailyConsumption <= 0)
                throw new ArgumentException("Daily consumption must be greater than zero.");

            double runtimeDays = availableEnergy / dailyConsumption;

            int fullDays = (int)Math.Floor(runtimeDays);
            double nextDayPercent = (runtimeDays - fullDays) * 100;

            return new CalculationResultDto
            {
                ContinuousMode = false,
                FullDayAvailable = fullDays >= 1,
                DailyConsumption = dailyConsumption,
                RuntimeDays = runtimeDays,
                AdditionalFullDays = Math.Max(0, fullDays - 1),
                NextDayPercent = nextDayPercent
            };
        }
    }
}
