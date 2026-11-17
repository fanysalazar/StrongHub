using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class BadgeControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    private readonly InMemoryRepository<UserBadge> _badges = new();
    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly InMemoryVisitRepository _visits = new();
    private readonly SessionContext _session = new();
    private readonly BadgeController _controller;

    public BadgeControllerTests()
    {
        _session.SignIn(new BasicUser
        {
            Id = 1, Name = "Ismael Ulate", Username = "ismael", GoalMuscleKg = 40, GoalBodyFatPercent = 18
        });

        var evaluator = new BadgeEvaluator(new IBadgeRule[]
        {
            new MuscleGoalRule(),
            new BodyFatGoalRule(),
            new VisitCountRule("regular-visitor", "Regular visitor", 6, 30),
            new VisitCountRule("gym-devotee", "Gym devotee", 12, 30)
        });

        _controller = new BadgeController(
            _badges, evaluator, new BadgeContextFactory(_measurements, _visits), _session, () => Today);
    }

    private void AddVisits(int count)
    {
        for (int day = 1; day <= count; day++)
        {
            _visits.Add(new Visit { UserId = 1, ClassName = "Yoga", Date = Today.AddDays(-day) });
        }
    }

    [Fact]
    public void EvaluateAchievements_MuscleGoalReached_SavesTheBadgeAndRaisesTheEvent()
    {
        _measurements.Add(new Measurement { UserId = 1, Date = Today.AddDays(-3), MuscleKg = 41, BodyFatPercent = 25 });
        BadgeEarnedEventArgs? raised = null;
        _controller.BadgeEarned += (_, args) => raised = args;

        IReadOnlyList<UserBadge> awarded = _controller.EvaluateAchievements();

        UserBadge badge = Assert.Single(awarded);
        Assert.Equal("muscle-goal", badge.BadgeCode);
        Assert.Equal(Today, badge.AwardedOn);
        Assert.Single(_badges.GetAll());
        Assert.NotNull(raised);
        Assert.Equal("Ismael Ulate", raised!.MemberName);
    }

    [Fact]
    public void EvaluateAchievements_CalledTwice_NeverAwardsTheSameBadgeTwice()
    {
        _measurements.Add(new Measurement { UserId = 1, Date = Today.AddDays(-3), MuscleKg = 41, BodyFatPercent = 25 });
        int timesRaised = 0;
        _controller.BadgeEarned += (_, _) => timesRaised++;

        _controller.EvaluateAchievements();
        IReadOnlyList<UserBadge> second = _controller.EvaluateAchievements();

        Assert.Empty(second);
        Assert.Single(_badges.GetAll());
        Assert.Equal(1, timesRaised);
    }

    [Fact]
    public void EvaluateAchievements_MemberWithoutData_EarnsNothing()
    {
        Assert.Empty(_controller.EvaluateAchievements());
    }

    [Fact]
    public void EvaluateAchievements_SixVisitsInThirtyDays_AwardsOnlyRegularVisitor()
    {
        AddVisits(6);

        IReadOnlyList<UserBadge> awarded = _controller.EvaluateAchievements();

        UserBadge badge = Assert.Single(awarded);
        Assert.Equal("regular-visitor", badge.BadgeCode);
    }

    [Fact]
    public void EvaluateAchievements_TwelveVisitsInThirtyDays_AwardsBothVisitBadges()
    {
        AddVisits(12);

        IReadOnlyList<UserBadge> awarded = _controller.EvaluateAchievements();

        Assert.Contains(awarded, badge => badge.BadgeCode == "regular-visitor");
        Assert.Contains(awarded, badge => badge.BadgeCode == "gym-devotee");
    }

    [Fact]
    public void EvaluateAchievements_AsTrainer_ThrowsValidationException()
    {
        _session.SignIn(new Trainer { Id = 3, Name = "Greivin Segura", Username = "greivin" });

        Assert.Throws<ValidationException>(() => _controller.EvaluateAchievements());
    }

    [Fact]
    public void GetMyBadges_ReturnsOnlyOwnBadgesNewestFirst()
    {
        _badges.Add(new UserBadge { UserId = 1, BadgeName = "Older", Kind = BadgeKind.Achievement, AwardedOn = Today.AddDays(-10) });
        _badges.Add(new UserBadge { UserId = 1, BadgeName = "Newer", Kind = BadgeKind.Shame, AwardedOn = Today.AddDays(-1) });
        _badges.Add(new UserBadge { UserId = 2, BadgeName = "Someone else", Kind = BadgeKind.Achievement, AwardedOn = Today });

        IReadOnlyList<BadgeRow> rows = _controller.GetMyBadges();

        Assert.Equal(2, rows.Count);
        Assert.Equal("Newer", rows[0].Name);
        Assert.Equal("Shame", rows[0].Type);
    }

    [Fact]
    public void GetSummary_AfterEarningOneBadge_CountsItAgainstTheAvailableOnes()
    {
        _measurements.Add(new Measurement { UserId = 1, Date = Today.AddDays(-3), MuscleKg = 41, BodyFatPercent = 25 });
        _controller.EvaluateAchievements();

        Assert.Equal("Achievement badges: 1 of 4 earned. Shame badges received: 0.", _controller.GetSummary());
    }
}
