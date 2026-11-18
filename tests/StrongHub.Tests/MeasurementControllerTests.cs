using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class MeasurementControllerTests
{
    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly SessionContext _session = new();
    private readonly MeasurementController _controller;

    public MeasurementControllerTests()
    {
        _session.SignIn(new BasicUser { Id = 1, Name = "Yaneth Cordero", Username = "yaneth" });
        _controller = new MeasurementController(_measurements, _session);
    }

    private void Add(int userId, DateOnly date, double weight, double fat, double muscle) =>
        _measurements.Add(new Measurement
        {
            UserId = userId, Date = date, WeightKg = weight, BodyFatPercent = fat, MuscleKg = muscle
        });

    [Fact]
    public void GetRows_ReturnsOnlyOwnMeasurementsNewestFirst()
    {
        Add(1, new DateOnly(2026, 11, 1), 80, 25, 35);
        Add(1, new DateOnly(2026, 11, 15), 78.5, 23.5, 36);
        Add(2, new DateOnly(2026, 11, 20), 70, 20, 40);

        IReadOnlyList<MeasurementRow> rows = _controller.GetRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal("2026-11-15", rows[0].Date);
        Assert.Equal(78.5, rows[0].WeightKg);
        Assert.Equal(23.5, rows[0].BodyFatPercent);
        Assert.Equal(36, rows[0].MuscleKg);
    }

    [Fact]
    public void GetRows_SignedInAsTrainer_ReturnsNothing()
    {
        Add(1, new DateOnly(2026, 11, 1), 80, 25, 35);
        _session.SignIn(new Trainer { Id = 1, Name = "Rosaura Chacon", Username = "rosaura" });

        Assert.Empty(_controller.GetRows());
    }

    [Fact]
    public void GetSummary_WithoutMeasurements_SaysSo()
    {
        Assert.Equal("No measurements yet.", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_OneMeasurement_JustCountsIt()
    {
        Add(1, new DateOnly(2026, 11, 1), 80, 25, 35);

        Assert.Equal("1 measurement recorded.", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_TwoMeasurements_ShowsTheChangeSinceTheFirstOne()
    {
        Add(1, new DateOnly(2026, 11, 15), 78.5, 23.5, 36);
        Add(1, new DateOnly(2026, 11, 1), 80, 25, 35);

        Assert.Equal(
            "2 measurements. Change since 2026-11-01: weight -1.5 kg, body fat -1.5 %, muscle +1 kg",
            _controller.GetSummary());
    }
}
