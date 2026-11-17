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

            if (accumulatorList.Count == 0)
                throw new ArgumentException("At least one accumulator is required.");

            EnsureValidAccumulators(accumulatorList);
            EnsureValidDevices(deviceList);

            double availableEnergy = CalculateAvailableEnergy(accumulatorList);

            bool continuousMode = deviceList.All(d =>
                d.HoursPerDay == SchemeInputValidator.ContinuousHoursMarker);

            if (continuousMode)
            {
                double totalPower = deviceList.Sum(d => d.PowerWatts * d.Quantity);

                return new CalculationResultDto
                {
                    ContinuousMode = true,
                    RuntimeHours = availableEnergy / totalPower
                };
            }

            double dailyConsumption = CalculateDailyConsumption(deviceList);

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
            var deviceList = devices.ToList();
            var accumulatorList = currentAccumulators.ToList();

            if (!double.IsFinite(targetDays) || targetDays <= 0)
                throw new ArgumentException("Target days must be greater than zero.");

            if (!SchemeInputValidator.IsValidEfficiency(efficiencyPercent))
                throw new ArgumentException("Efficiency must be in range (0; 100].");

            EnsureValidDevices(deviceList);

            EnsureValidAccumulators(accumulatorList);

            double dailyConsumption = CalculateDailyConsumption(deviceList);

            double efficiency = efficiencyPercent / 100;

            double requiredEnergy = dailyConsumption * targetDays;
            double requiredCapacity = requiredEnergy / efficiency / 1000;

            double currentAvailableEnergy = CalculateAvailableEnergy(accumulatorList);
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

        private static void EnsureValidDevices(List<Device> devices)
        {
            if (devices.Count == 0)
                throw new ArgumentException("At least one device is required.");

            if (devices.Any(d => !SchemeInputValidator.IsValidDeviceValues(
                    d.PowerWatts, d.Quantity, d.HoursPerDay)))
            {
                throw new ArgumentException("Invalid device values.");
            }
        }

        private static void EnsureValidAccumulators(IEnumerable<Accumulator> accumulators)
        {
            if (accumulators.Any(a => !SchemeInputValidator.IsValidAccumulatorValues(
                    a.CapacityKilowattHours, a.EfficiencyPercent)))
            {
                throw new ArgumentException("Invalid accumulator values.");
            }
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