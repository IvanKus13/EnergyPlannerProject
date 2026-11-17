using EnergyPlanner.Services;
using EnergyPlannerProject.DTO;

namespace EnergyPlanner.Tests;

public class SchemeInputValidatorTests
{
    private static CreateDeviceDto CreateDevice(
        double powerWatts = 100,
        int quantity = 1,
        double hoursPerDay = 10,
        string name = "Лампа") => new()
        {
            Name = name,
            PowerWatts = powerWatts,
            Quantity = quantity,
            HoursPerDay = hoursPerDay
        };

    private static CreateAccumulatorDto CreateAccumulator(
        double capacityKilowattHours = 5,
        double efficiencyPercent = 90,
        string name = "АКБ") => new()
        {
            Name = name,
            CapacityKilowattHours = capacityKilowattHours,
            EfficiencyPercent = efficiencyPercent
        };

    private static CapacityRecommendationRequestDto CreateValidRequest() => new()
    {
        Devices = new List<CreateDeviceDto> { CreateDevice() },
        Accumulators = new List<CreateAccumulatorDto>(),
        TargetDays = 1,
        EfficiencyPercent = 90
    };


    [Theory]
    [InlineData(24.0)]                                        
    [InlineData(0.5)]
    [InlineData(SchemeInputValidator.ContinuousHoursMarker)]   
    public void IsValidDevice_AcceptsAllowedHours(double hoursPerDay)
    {
        Assert.True(SchemeInputValidator.IsValidDevice(CreateDevice(hoursPerDay: hoursPerDay)));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(24.5)]          
    [InlineData(999.0)]        
    [InlineData(double.NaN)]
    public void IsValidDevice_RejectsInvalidHours(double hoursPerDay)
    {
        Assert.False(SchemeInputValidator.IsValidDevice(CreateDevice(hoursPerDay: hoursPerDay)));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void IsValidDevice_RejectsInvalidPower(double powerWatts)
    {
        Assert.False(SchemeInputValidator.IsValidDevice(CreateDevice(powerWatts: powerWatts)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValidDevice_RejectsInvalidQuantity(int quantity)
    {
        Assert.False(SchemeInputValidator.IsValidDevice(CreateDevice(quantity: quantity)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidDevice_RejectsEmptyName(string name)
    {
        Assert.False(SchemeInputValidator.IsValidDevice(CreateDevice(name: name)));
    }

    [Fact]
    public void IsValidDevice_RejectsNull()
    {
        Assert.False(SchemeInputValidator.IsValidDevice(null!));
    }


    [Fact]
    public void IsValidAccumulator_AcceptsCorrectAccumulator()
    {
        Assert.True(SchemeInputValidator.IsValidAccumulator(CreateAccumulator()));
    }

    [Theory]
    [InlineData(0.0, 90.0)]    
    [InlineData(-1.0, 90.0)]    
    [InlineData(5.0, 0.0)]      
    [InlineData(5.0, 100.1)]    
    public void IsValidAccumulator_RejectsInvalidValues(double capacityKilowattHours, double efficiencyPercent)
    {
        var accumulator = CreateAccumulator(capacityKilowattHours, efficiencyPercent);

        Assert.False(SchemeInputValidator.IsValidAccumulator(accumulator));
    }

    [Fact]
    public void IsValidAccumulator_RejectsEmptyName()
    {
        Assert.False(SchemeInputValidator.IsValidAccumulator(CreateAccumulator(name: " ")));
    }


    [Theory]
    [InlineData(0.1, true)]
    [InlineData(50.0, true)]
    [InlineData(100.0, true)]                  
    [InlineData(0.0, false)]                  
    [InlineData(-10.0, false)]
    [InlineData(100.01, false)]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    public void IsValidEfficiency_ChecksRange(double efficiencyPercent, bool expected)
    {
        Assert.Equal(expected, SchemeInputValidator.IsValidEfficiency(efficiencyPercent));
    }


    [Fact]
    public void ValidateRecommendationRequest_AcceptsValidRequest()
    {
        Assert.Null(SchemeInputValidator.ValidateRecommendationRequest(CreateValidRequest()));
    }

    [Fact]
    public void ValidateRecommendationRequest_RejectsNull()
    {
        Assert.NotNull(SchemeInputValidator.ValidateRecommendationRequest(null));
    }

    [Fact]
    public void ValidateRecommendationRequest_RejectsRequestWithoutDevices()
    {
        var request = CreateValidRequest();
        request.Devices.Clear();

        Assert.NotNull(SchemeInputValidator.ValidateRecommendationRequest(request));
    }

    [Fact]
    public void ValidateRecommendationRequest_AllowsMissingAccumulators()
    {
        var request = CreateValidRequest();
        request.Accumulators = null!;

        Assert.Null(SchemeInputValidator.ValidateRecommendationRequest(request));
    }

    [Fact]
    public void ValidateRecommendationRequest_RejectsInvalidDevice()
    {
        var request = CreateValidRequest();
        request.Devices.Add(CreateDevice(powerWatts: 0));

        Assert.NotNull(SchemeInputValidator.ValidateRecommendationRequest(request));
    }

    [Fact]
    public void ValidateRecommendationRequest_RejectsInvalidAccumulator()
    {
        var request = CreateValidRequest();
        request.Accumulators.Add(CreateAccumulator(capacityKilowattHours: 0));

        Assert.NotNull(SchemeInputValidator.ValidateRecommendationRequest(request));
    }

    [Theory]
    [InlineData(0.5, true)]
    [InlineData(365.0, true)]       
    [InlineData(0.0, false)]
    [InlineData(-1.0, false)]
    [InlineData(365.5, false)]
    [InlineData(366.0, false)]
    public void ValidateRecommendationRequest_ChecksTargetDays(double targetDays, bool isValid)
    {
        var request = CreateValidRequest();
        request.TargetDays = targetDays;

        var error = SchemeInputValidator.ValidateRecommendationRequest(request);

        Assert.Equal(isValid, error == null);
    }

    [Fact]
    public void ValidateRecommendationRequest_RejectsInvalidEfficiency()
    {
        var request = CreateValidRequest();
        request.EfficiencyPercent = 0;

        Assert.NotNull(SchemeInputValidator.ValidateRecommendationRequest(request));
    }
}