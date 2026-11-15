
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

            double availableEnergy = CalculateAvailableEnergy(accumulatorList);

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

            double dailyConsumption = CalculateDailyConsumption(deviceList);

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

        public CapacityRecommendationDto RecommendCapacity(
            IEnumerable<Device> devices,
            IEnumerable<Accumulator> currentAccumulators,
            double targetDays,
            double efficiencyPercent)
        {
            if (!double.IsFinite(targetDays) || targetDays <= 0)
                throw new ArgumentException("Target days must be greater than zero.");

            if (!SchemeInputValidator.IsValidEfficiency(efficiencyPercent))
                throw new ArgumentException("Efficiency must be in range (0; 100].");

            double dailyConsumption = CalculateDailyConsumption(devices);

            if (dailyConsumption <= 0)
                throw new ArgumentException("Daily consumption must be greater than zero.");

            double efficiency = efficiencyPercent / 100;

            double requiredEnergy = dailyConsumption * targetDays;
            double requiredCapacity = requiredEnergy / efficiency / 1000;

            double currentAvailableEnergy = CalculateAvailableEnergy(currentAccumulators);
            double missingEnergy = Math.Max(0, requiredEnergy - currentAvailableEnergy);
            double additionalCapacity = missingEnergy / efficiency / 1000;

            return new CapacityRecommendationDto
            {
                TargetDays = targetDays,
                EfficiencyPercent = efficiencyPercent,
                DailyConsumption = dailyConsumption,
                RequiredEnergy = requiredEnergy,
                RequiredCapacityKilowattHours = requiredCapacity,
                CurrentAvailableEnergy = currentAvailableEnergy,
                AdditionalCapacityKilowattHours = additionalCapacity,
                IsCurrentCapacityEnough = currentAvailableEnergy >= requiredEnergy
            };
        }

        private static double CalculateAvailableEnergy(IEnumerable<Accumulator> accumulators)
        {
            double total = 0;

            foreach (var accumulator in accumulators)
            {
                total += accumulator.CapacityKilowattHours * 1000 * (accumulator.EfficiencyPercent / 100);
            }

            return total;
        }

        private static double CalculateDailyConsumption(IEnumerable<Device> devices)
        {
            double total = 0;

            foreach (var device in devices)
            {
                double hours = device.HoursPerDay == SchemeInputValidator.ContinuousHoursMarker
                    ? 24
                    : device.HoursPerDay;

                total += device.PowerWatts * device.Quantity * hours;
            }

            return total;
        }
    }
}
