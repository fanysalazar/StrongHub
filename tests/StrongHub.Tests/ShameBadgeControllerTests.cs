using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class ShameBadgeControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateOnly RangeStart = new(2026, 11, 1);

    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryRepository<UserBadge> _badges = new();
    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly InMemoryVisitRepository _visits = new();
    private readonly SessionContext _session = new();

    public ShameBadgeControllerTests()
    {
        _session.SignIn(new Trainer { Id = 9, Name = "Eliecer Fonseca", Username = "eliecer" });
    }

    private ShameBadgeController CreateController(params IBadgeRule[] rules) =>
        new(
            _users,
            _badges,
            _visits,
            new BadgeEvaluator(rules),
            new BadgeContextFactory(_measurements, _visits),
            _session,
            () => Today);

    private void AddTwelveMembersWithIncreasingVisits()
    {
        for (int id = 1; id <= 12; id++)
        {
            _users.Add(new BasicUser { Name = $"Tester {id}", Username = $"tester{id}" });

            for (int visit = 1; visit <= id; visit++)
            {
                _visits.Add(new Visit { UserId = id, ClassName = "Yoga", Date = new DateOnly(2026, 11, 5) });
            }
        }
    }

    [Fact]
    public void Evaluate_FewestVisitsRule_AssignsOnlyToTheTenLowestMembers()
    {
        AddTwelveMembersWithIncreasingVisits();
        ShameBadgeController controller = CreateController(new LowestVisitsRule());

        ShameResult result = controller.Evaluate(RangeStart, Today);

        Assert.Equal(10, result.NewlyAssigned);
        Assert.Equal(10, result.Rows.Count);
        Assert.Contains(result.Rows, row => row.Member == "Tester 1");
        Assert.DoesNotContain(result.Rows, row => row.Member == "Tester 11" || row.Member == "Tester 12");
    }

    [Fact]
    public void Evaluate_SamePeriodTwice_DoesNotDuplicateBadgesOrEvents()
    {
        AddTwelveMembersWithIncreasingVisits();
        ShameBadgeController controller = CreateController(new LowestVisitsRule());
        int timesRaised = 0;
        controller.ShameBadgeAssigned += (_, _) => timesRaised++;

        controller.Evaluate(RangeStart, Today);
        ShameResult second = controller.Evaluate(RangeStart, Today);

        Assert.Equal(0, second.NewlyAssigned);
        Assert.Equal(10, second.Rows.Count);
        Assert.Equal(10, timesRaised);
        Assert.Equal(10, _badges.GetAll().Count);
    }

    [Fact]
    public void Evaluate_AsMember_ThrowsValidationException()
    {
        ShameBadgeController controller = CreateController(new LowestVisitsRule());
        _session.SignIn(new BasicUser { Id = 1, Name = "Tester 1", Username = "tester1" });

        Assert.False(controller.CanEvaluate);
        Assert.Throws<ValidationException>(() => controller.Evaluate(RangeStart, Today));
    }

    [Fact]
    public void Evaluate_StartAfterEnd_ThrowsValidationException()
    {
        ShameBadgeController controller = CreateController(new LowestVisitsRule());

        Assert.Throws<ValidationException>(() => controller.Evaluate(Today, RangeStart));
    }

    [Fact]
    public void Evaluate_ProgressAndGoalRules_FlagOnlyTheMemberWithoutResults()
    {
        _users.Add(new BasicUser { Name = "Tester A", Username = "a", GoalMuscleKg = 40, GoalBodyFatPercent = 18 });
        _users.Add(new BasicUser { Name = "Tester B", Username = "b", GoalMuscleKg = 40, GoalBodyFatPercent = 18 });
        _measurements.Add(new Measurement { UserId = 1, Date = new DateOnly(2026, 11, 3), MuscleKg = 30, BodyFatPercent = 25 });
        _measurements.Add(new Measurement { UserId = 1, Date = new DateOnly(2026, 11, 15), MuscleKg = 41, BodyFatPercent = 17 });
        ShameBadgeController controller = CreateController(new NoProgressRule(), new GoalsNotMetRule());

        ShameResult result = controller.Evaluate(RangeStart, Today);

        Assert.Equal(2, result.NewlyAssigned);
        Assert.All(result.Rows, row => Assert.Equal("Tester B", row.Member));
    }

    [Fact]
    public void GetAssignedRows_AnotherPeriod_ReturnsNothing()
    {
        AddTwelveMembersWithIncreasingVisits();
        ShameBadgeController controller = CreateController(new LowestVisitsRule());
        controller.Evaluate(RangeStart, Today);

        Assert.Empty(controller.GetAssignedRows(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)));
        Assert.Equal(10, controller.GetAssignedRows(RangeStart, Today).Count);
    }
}
