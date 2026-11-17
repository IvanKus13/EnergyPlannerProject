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


    [Fact]
    public void RecommendCapacity_CalculatesRequiredAndMissingCapacity()
    {
        var result = _service.RecommendCapacity(
            new[] { Appliance(100, 24) },
            new[] { Battery(2, 90) },
            2,
            80);

        Assert.Equal(2400, result.DailyConsumption, 6);
        Assert.Equal(4800, result.RequiredEnergy, 6);
        Assert.Equal(6, result.RequiredCapacityKilowattHours, 6);
        Assert.Equal(1800, result.CurrentAvailableEnergy, 6);
        Assert.Equal(3.75, result.AdditionalCapacityKilowattHours, 6);
        Assert.False(result.IsCurrentCapacityEnough);
    }

    [Fact]
    public void RecommendCapacity_CurrentCapacityIsEnough_NoAdditionalCapacity()
    {
        var result = _service.RecommendCapacity(
            new[] { Appliance(100, 10) },
            new[] { Battery(2, 100) },
            1,
            90);

        Assert.True(result.IsCurrentCapacityEnough);
        Assert.Equal(1000, result.RequiredEnergy, 6);
        Assert.Equal(0, result.AdditionalCapacityKilowattHours, 6);
    }

    [Fact]
    public void RecommendCapacity_WithoutAccumulators_AllCapacityIsAdditional()
    {
        var result = _service.RecommendCapacity(
            new[] { Appliance(50, 24) },
            Array.Empty<Accumulator>(),
            0.5,
            100);

        Assert.Equal(0, result.CurrentAvailableEnergy, 6);
        Assert.Equal(0.6, result.RequiredCapacityKilowattHours, 6);
        Assert.Equal(0.6, result.AdditionalCapacityKilowattHours, 6);
        Assert.False(result.IsCurrentCapacityEnough);
    }

    [Fact]
    public void RecommendCapacity_CountsContinuousAs24Hours()
    {
        var result = _service.RecommendCapacity(
            new[] { Appliance(100, Continuous) },
            Array.Empty<Accumulator>(),
            1,
            100);

        Assert.Equal(2400, result.DailyConsumption, 6);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RecommendCapacity_InvalidTargetDays_Throws(double targetDays)
    {
        Assert.Throws<ArgumentException>(() => _service.RecommendCapacity(
            new[] { Appliance(100, 10) },
            Array.Empty<Accumulator>(),
            targetDays,
            90));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    [InlineData(100.5)]
    [InlineData(double.NaN)]
    public void RecommendCapacity_InvalidEfficiency_Throws(double efficiencyPercent)
    {
        Assert.Throws<ArgumentException>(() => _service.RecommendCapacity(
            new[] { Appliance(100, 10) },
            Array.Empty<Accumulator>(),
            1,
            efficiencyPercent));
    }



    [Theory]
    [InlineData(double.NaN, 10.0)]
    [InlineData(double.PositiveInfinity, 10.0)]
    [InlineData(100.0, double.NaN)]
    [InlineData(100.0, double.PositiveInfinity)]
    public void CalculateRuntime_NonFiniteDeviceValues_Throws(double powerWatts, double hoursPerDay)
    {
        Assert.Throws<ArgumentException>(() => _service.CalculateRuntime(
            new[] { Battery(5, 90) },
            new[] { Appliance(powerWatts, hoursPerDay) }));
    }

    [Theory]
    [InlineData(-2.0, 90.0)]                    
    [InlineData(0.0, 90.0)]
    [InlineData(double.NaN, 90.0)]
    [InlineData(double.PositiveInfinity, 90.0)]
    [InlineData(5.0, 0.0)]                      
    [InlineData(5.0, 150.0)]                    
    [InlineData(5.0, double.NaN)]
    public void CalculateRuntime_InvalidAccumulatorAmongValid_Throws(
        double capacityKilowattHours, double efficiencyPercent)
    {
        Assert.Throws<ArgumentException>(() => _service.CalculateRuntime(
            new[] { Battery(5, 90), Battery(capacityKilowattHours, efficiencyPercent) },
            new[] { Appliance(100, 10) }));
    }

    [Theory]
    [InlineData(double.NaN, 10.0)]
    [InlineData(double.PositiveInfinity, 10.0)]
    [InlineData(100.0, double.NaN)]
    [InlineData(100.0, 25.0)]
    public void RecommendCapacity_InvalidDeviceValues_Throws(double powerWatts, double hoursPerDay)
    {
        Assert.Throws<ArgumentException>(() => _service.RecommendCapacity(
            new[] { Appliance(powerWatts, hoursPerDay) },
            Array.Empty<Accumulator>(),
            1,
            90));
    }

    [Theory]
    [InlineData(-2.0, 90.0)]
    [InlineData(double.NaN, 90.0)]
    [InlineData(5.0, 150.0)]
    public void RecommendCapacity_InvalidAccumulatorAmongValid_Throws(
        double capacityKilowattHours, double efficiencyPercent)
    {
        Assert.Throws<ArgumentException>(() => _service.RecommendCapacity(
            new[] { Appliance(100, 10) },
            new[] { Battery(5, 90), Battery(capacityKilowattHours, efficiencyPercent) },
            1,
            90));
    }
}