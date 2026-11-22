using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class StatisticsServiceTests
{
    private static readonly DateRange Period = new(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 18));

    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryVisitRepository _visits = new();
    private readonly StatisticsService _service;

    public StatisticsServiceTests()
    {
        _service = new StatisticsService(_measurements, _users, _visits);
    }

    private static BasicUser Member() =>
        new() { Id = 1, Name = "Ismenia Rojas", Username = "ismenia", GoalMuscleKg = 40, GoalBodyFatPercent = 18 };

    private void AddMeasurement(int userId, DateOnly date, double weight, double fat, double muscle) =>
        _measurements.Add(new Measurement
        {
            UserId = userId, Date = date, WeightKg = weight, BodyFatPercent = fat, MuscleKg = muscle
        });

    private void AddVisits(int userId, int count, DateOnly date)
    {
        for (int number = 0; number < count; number++)
        {
            _visits.Add(new Visit { UserId = userId, ClassName = "Yoga", Date = date });
        }
    }

    private void AddMembersWhereVisitsEqualTheId(int total)
    {
        for (int id = 1; id <= total; id++)
        {
            _users.Add(new BasicUser { Name = $"Tester {id}", Username = $"tester{id}" });
            AddVisits(id, id, new DateOnly(2026, 11, 5));
        }
    }

    [Fact]
    public void GetEvolution_TwoMeasurements_ReturnsStartEndChangeAndGoalProgress()
    {
        AddMeasurement(1, new DateOnly(2026, 11, 3), 80, 25, 30);
        AddMeasurement(1, new DateOnly(2026, 11, 15), 78.5, 22, 35);

        EvolutionResult result = _service.GetEvolution(Member(), Period);

        Assert.Equal("2 measurements between 2026-11-03 and 2026-11-15.", result.Summary);

        StatRow weight = result.Rows[0];
        Assert.Equal("Weight (kg)", weight.Metric);
        Assert.Equal("80", weight.Start);
        Assert.Equal("78.5", weight.End);
        Assert.Equal("-1.5", weight.Change);

        StatRow muscle = result.Rows[1];
        Assert.Equal("+5", muscle.Change);
        Assert.Equal("40", muscle.Goal);
        Assert.Equal("50% of the way, 5 kg to go", muscle.Progress);

        StatRow fat = result.Rows[2];
        Assert.Equal("-3", fat.Change);
        Assert.Equal("18", fat.Goal);
        Assert.Equal("43% of the way, 4 % to go", fat.Progress);
    }

    [Fact]
    public void GetEvolution_GoalsReached_ShowsGoalReached()
    {
        AddMeasurement(1, new DateOnly(2026, 11, 3), 80, 25, 36);
        AddMeasurement(1, new DateOnly(2026, 11, 15), 76, 17, 41);

        EvolutionResult result = _service.GetEvolution(Member(), Period);

        Assert.Equal("Goal reached", result.Rows[1].Progress);
        Assert.Equal("Goal reached", result.Rows[2].Progress);
    }

    [Fact]
    public void GetEvolution_LessThanTwoMeasurementsInTheRange_ExplainsWhy()
    {
        AddMeasurement(1, new DateOnly(2026, 10, 20), 80, 25, 30);
        AddMeasurement(1, new DateOnly(2026, 11, 15), 78, 24, 31);

        EvolutionResult result = _service.GetEvolution(Member(), Period);

        Assert.Equal("Not enough measurements in this period: at least 2 are needed, found 1.", result.Summary);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void GetEvolution_IgnoresMeasurementsOfOtherMembers()
    {
        AddMeasurement(2, new DateOnly(2026, 11, 3), 80, 25, 30);
        AddMeasurement(2, new DateOnly(2026, 11, 15), 78, 24, 31);

        EvolutionResult result = _service.GetEvolution(Member(), Period);

        Assert.Empty(result.Rows);
        Assert.Contains("found 0", result.Summary);
    }

    [Fact]
    public void GetMostVisits_ReturnsTheTenHighestInOrder()
    {
        AddMembersWhereVisitsEqualTheId(12);

        IReadOnlyList<RankingRow> rows = _service.GetMostVisits(Period, 10);

        Assert.Equal(10, rows.Count);
        Assert.Equal(1, rows[0].Position);
        Assert.Equal("Tester 12", rows[0].Member);
        Assert.Equal(12, rows[0].Visits);
        Assert.Equal("Tester 3", rows[9].Member);
    }

    [Fact]
    public void GetFewestVisits_ReturnsTheTenLowestInOrder()
    {
        AddMembersWhereVisitsEqualTheId(12);

        IReadOnlyList<RankingRow> rows = _service.GetFewestVisits(Period, 10);

        Assert.Equal(10, rows.Count);
        Assert.Equal("Tester 1", rows[0].Member);
        Assert.Equal(1, rows[0].Visits);
        Assert.Equal("Tester 10", rows[9].Member);
    }

    [Fact]
    public void GetMostVisits_TiesAreBrokenById()
    {
        for (int id = 1; id <= 3; id++)
        {
            _users.Add(new BasicUser { Name = $"Tester {id}", Username = $"tester{id}" });
            AddVisits(id, 2, new DateOnly(2026, 11, 5));
        }

        IReadOnlyList<RankingRow> rows = _service.GetMostVisits(Period, 3);

        Assert.Equal("Tester 1", rows[0].Member);
        Assert.Equal("Tester 2", rows[1].Member);
        Assert.Equal("Tester 3", rows[2].Member);
    }

    [Fact]
    public void CountVisits_IgnoresVisitsOutsideThePeriod()
    {
        AddVisits(1, 1, new DateOnly(2026, 11, 5));
        AddVisits(1, 1, new DateOnly(2026, 10, 30));
        AddVisits(1, 1, new DateOnly(2026, 11, 19));

        Assert.Equal(1, _service.CountVisits(1, Period));
    }

    [Fact]
    public void GetPosition_MostAndFewestVisits_AreFirstAndLast()
    {
        AddMembersWhereVisitsEqualTheId(12);

        Assert.Equal(1, _service.GetPosition(12, Period));
        Assert.Equal(12, _service.GetPosition(1, Period));
    }

    [Fact]
    public void GetPosition_MembersWithTheSameVisitsShareThePosition()
    {
        _users.Add(new BasicUser { Name = "Tester A", Username = "a" });
        _users.Add(new BasicUser { Name = "Tester B", Username = "b" });
        _users.Add(new BasicUser { Name = "Tester C", Username = "c" });
        AddVisits(1, 3, new DateOnly(2026, 11, 5));
        AddVisits(2, 3, new DateOnly(2026, 11, 6));
        AddVisits(3, 1, new DateOnly(2026, 11, 7));

        Assert.Equal(1, _service.GetPosition(1, Period));
        Assert.Equal(1, _service.GetPosition(2, Period));
        Assert.Equal(3, _service.GetPosition(3, Period));
    }
}
