using StrongHub.Controllers;
using Xunit;

namespace StrongHub.Tests;

public class WeekHelperTests
{
    [Theory]
    [InlineData(2026, 11, 16, 2026, 11, 16)]
    [InlineData(2026, 11, 18, 2026, 11, 16)]
    [InlineData(2026, 11, 22, 2026, 11, 16)]
    public void StartOfWeek_ReturnsTheMondayOfThatWeek(int y, int m, int d, int expectedY, int expectedM, int expectedD)
    {
        Assert.Equal(new DateOnly(expectedY, expectedM, expectedD), WeekHelper.StartOfWeek(new DateOnly(y, m, d)));
    }
}
