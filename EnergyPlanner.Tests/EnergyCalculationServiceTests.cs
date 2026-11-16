using EnergyPlanner.Services;
using EnergyPlannerProject.Models;

namespace EnergyPlanner.Tests;

public class EnergyCalculationServiceTests
{
    private const double Continuous = SchemeInputValidator.ContinuousHoursMarker;

    private readonly EnergyCalculationService _service = new();

    private static Accumulator Battery(double capacityKilowattHours, double efficiencyPercent) => new()
    {
        Name = "Акумулятор",
        CapacityKilowattHours = capacityKilowattHours,
        EfficiencyPercent = efficiencyPercent
    };

    private static Device Appliance(double powerWatts, double hoursPerDay, int quantity = 1) => new()
    {
        Name = "Пристрій",
        PowerWatts = powerWatts,
        Quantity = quantity,
        HoursPerDay = hoursPerDay
    };


    [Fact]
    public void CalculateRuntime_AllDevicesContinuous_ReturnsRuntimeInHours()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            new[] { Appliance(150, Continuous) });

        Assert.True(result.ContinuousMode);
        Assert.Equal(30, result.RuntimeHours, 6);
    }

    [Fact]
    public void CalculateRuntime_ContinuousMode_TakesQuantityIntoAccount()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            new[] { Appliance(75, Continuous, quantity: 2) });

        Assert.Equal(30, result.RuntimeHours, 6);
    }

    [Fact]
    public void CalculateRuntime_SumsEnergyOfAllAccumulators()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(2, 90), Battery(3, 80) },
            new[] { Appliance(100, Continuous) });

        Assert.Equal(42, result.RuntimeHours, 6);
    }


    [Fact]
    public void CalculateRuntime_SeveralDays_ReturnsFullDaysAndRemainder()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            new[] { Appliance(100, 12) });

        Assert.False(result.ContinuousMode);
        Assert.Equal(1200, result.DailyConsumption, 6);
        Assert.Equal(3.75, result.RuntimeDays, 6);
        Assert.True(result.FullDayAvailable);
        Assert.Equal(2, result.AdditionalFullDays);   
        Assert.Equal(75, result.NextDayPercent, 6);   
    }

    [Fact]
    public void CalculateRuntime_LessThanOneDay_FullDayNotAvailable()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(1, 100) },
            new[] { Appliance(100, 24) });

        Assert.False(result.FullDayAvailable);
        Assert.Equal(0, result.AdditionalFullDays);
        Assert.Equal(1000.0 / 2400, result.RuntimeDays, 6);
    }

    [Fact]
    public void CalculateRuntime_ExactlyOneDay_IsFullDayWithoutRemainder()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(2, 100) },
            new[] { Appliance(100, 20) });

        Assert.True(result.FullDayAvailable);
        Assert.Equal(0, result.AdditionalFullDays);
        Assert.Equal(0, result.NextDayPercent, 6);
    }

    [Fact]
    public void CalculateRuntime_MixedDevices_CountsContinuousAs24Hours()
    {
        var result = _service.CalculateRuntime(
            new[] { Battery(10, 100) },
            new[] { Appliance(100, Continuous), Appliance(50, 4) });

        Assert.False(result.ContinuousMode);
        Assert.Equal(2600, result.DailyConsumption, 6);
    }


    [Fact]
    public void CalculateRuntime_NoAccumulators_Throws()
    {
        Assert.Throws<ArgumentException>(() => _service.CalculateRuntime(
            Array.Empty<Accumulator>(),
            new[] { Appliance(100, 10) }));
    }

    [Fact]
    public void CalculateRuntime_NoDevices_Throws()
    {
        Assert.Throws<ArgumentException>(() => _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            Array.Empty<Device>()));
    }

    [Theory]
    [InlineData(0.0, 1, 10.0)]      
    [InlineData(100.0, 0, 10.0)]    
    [InlineData(100.0, 1, 0.0)]    
    [InlineData(100.0, 1, 25.0)]    
    public void CalculateRuntime_InvalidDevice_Throws(double powerWatts, int quantity, double hoursPerDay)
    {
        Assert.Throws<ArgumentException>(() => _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            new[] { Appliance(powerWatts, hoursPerDay, quantity) }));
    }
}