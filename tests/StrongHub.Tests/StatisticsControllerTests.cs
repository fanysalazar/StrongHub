using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class StatisticsControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateOnly Start = new(2026, 11, 1);

    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryVisitRepository _visits = new();
    private readonly SessionContext _session = new();
    private readonly StatisticsController _controller;

    public StatisticsControllerTests()
    {
        for (int id = 1; id <= 3; id++)
        {
            _users.Add(new BasicUser { Name = $"Tester {id}", Username = $"tester{id}", GoalMuscleKg = 40, GoalBodyFatPercent = 18 });

            for (int visit = 1; visit <= id; visit++)
            {
                _visits.Add(new Visit { UserId = id, ClassName = "Yoga", Date = new DateOnly(2026, 11, 5) });
            }
        }

        _session.SignIn(_users.GetAll()[0]);
        _controller = new StatisticsController(
            new StatisticsService(_measurements, _users, _visits), _session, () => Today);
    }

    [Fact]
    public void GetEvolution_Member_ReturnsTheThreeMetrics()
    {
        _measurements.Add(new Measurement { UserId = 1, Date = new DateOnly(2026, 11, 3), WeightKg = 80, BodyFatPercent = 25, MuscleKg = 30 });
        _measurements.Add(new Measurement { UserId = 1, Date = new DateOnly(2026, 11, 15), WeightKg = 79, BodyFatPercent = 24, MuscleKg = 32 });

        EvolutionResult result = _controller.GetEvolution(Start, Today);

        Assert.Equal(3, result.Rows.Count);
    }

    [Fact]
    public void GetEvolution_SignedInAsTrainer_ThrowsValidationException()
    {
        _session.SignIn(new Trainer { Id = 9, Name = "Eliecer Fonseca", Username = "eliecer" });

        Assert.False(_controller.CanView);
        Assert.Throws<ValidationException>(() => _controller.GetEvolution(Start, Today));
    }

    [Fact]
    public void GetEvolution_StartAfterEnd_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.GetEvolution(Today, Start));
    }

    [Fact]
    public void GetEvolution_EndInTheFuture_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.GetEvolution(Today, Today.AddDays(1)));
    }

    [Fact]
    public void GetRankings_Member_ReturnsBothListsAndTheMemberPosition()
    {
        RankingResult result = _controller.GetRankings(Start, Today);

        Assert.Equal("Tester 3", result.MostVisits[0].Member);
        Assert.Equal("Tester 1", result.FewestVisits[0].Member);
        Assert.Equal(
            "You attended 1 class(es) in this period. Your position: 3 of 3 members.",
            result.MySummary);
    }

    [Fact]
    public void GetRankings_SignedInAsTrainer_ThrowsValidationException()
    {
        _session.SignIn(new Trainer { Id = 9, Name = "Eliecer Fonseca", Username = "eliecer" });

        Assert.Throws<ValidationException>(() => _controller.GetRankings(Start, Today));
    }

    [Fact]
    public void GetRankings_StartAfterEnd_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.GetRankings(Today, Start));
    }
}
