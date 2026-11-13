using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class BmiCalculatorTests
{
    [Fact]
    public void Calculate_ValidInput_ReturnsRoundedBmi()
    {
        Assert.Equal(22.86, BmiCalculator.Calculate(70, 1.75));
    }

    [Theory]
    [InlineData(0, 1.75)]
    [InlineData(70, 0)]
    [InlineData(-5, 1.70)]
    public void Calculate_NonPositiveValues_ThrowsValidationException(double weight, double height)
    {
        Assert.Throws<ValidationException>(() => BmiCalculator.Calculate(weight, height));
    }
}
