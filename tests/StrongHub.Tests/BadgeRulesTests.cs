using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class BadgeRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    private static Measurement Meas(int day, double muscle, double fat) =>
        new() { UserId = 1, Date = new DateOnly(2026, 11, day), MuscleKg = muscle, BodyFatPercent = fat };

    private static Visit VisitOn(int daysFromToday) =>
        new() { UserId = 1, ClassName = "Yoga", Date = Today.AddDays(daysFromToday) };

    private static BadgeContext Context(IReadOnlyList<Measurement>? measurements = null, IReadOnlyList<Visit>? visits = null) =>
        new(
            new BasicUser { Id = 1, Name = "Ximena Arguedas", Username = "ximena", GoalMuscleKg = 40, GoalBodyFatPercent = 18 },
            measurements ?? new List<Measurement>(),
            visits ?? new List<Visit>(),
            Today,
            null,
            new HashSet<int>());

    private sealed class FakeRule : IBadgeRule
    {
        private readonly bool _satisfied;

        public FakeRule(string code, BadgeKind kind, bool satisfied)
        {
            Code = code;
            Kind = kind;
            _satisfied = satisfied;
        }

        public string Code { get; }
        public string Name => Code;
        public BadgeKind Kind { get; }
        public string Message => string.Empty;
        public bool IsSatisfied(BadgeContext context) => _satisfied;
    }

    [Fact]
    public void MuscleGoalRule_LatestMeasurementReachesGoal_IsSatisfied()
    {
        var measurements = new List<Measurement> { Meas(5, 38, 25), Meas(10, 41, 25) };

        Assert.True(new MuscleGoalRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void MuscleGoalRule_LatestBelowGoal_IsNotSatisfiedEvenIfAnEarlierOneWasAbove()
    {
        var measurements = new List<Measurement> { Meas(5, 42, 25), Meas(10, 38, 25) };

        Assert.False(new MuscleGoalRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void MuscleGoalRule_NoMeasurements_IsNotSatisfied()
    {
        Assert.False(new MuscleGoalRule().IsSatisfied(Context()));
    }

    [Fact]
    public void BodyFatGoalRule_LatestAtOrBelowGoal_IsSatisfied()
    {
        var measurements = new List<Measurement> { Meas(10, 35, 17) };

        Assert.True(new BodyFatGoalRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void BodyFatGoalRule_LatestAboveGoal_IsNotSatisfied()
    {
        var measurements = new List<Measurement> { Meas(10, 35, 20) };

        Assert.False(new BodyFatGoalRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void VisitCountRule_EnoughVisitsInsideTheWindow_IsSatisfied()
    {
        var rule = new VisitCountRule("test", "Test", 3, 30);
        var visits = new List<Visit> { VisitOn(-1), VisitOn(-5), VisitOn(-29) };

        Assert.True(rule.IsSatisfied(Context(visits: visits)));
    }

    [Fact]
    public void VisitCountRule_VisitsOlderThanTheWindow_AreNotCounted()
    {
        var rule = new VisitCountRule("test", "Test", 3, 30);
        var visits = new List<Visit> { VisitOn(-1), VisitOn(-5), VisitOn(-30) };

        Assert.False(rule.IsSatisfied(Context(visits: visits)));
    }

    [Fact]
    public void DateRange_StartAfterEnd_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => new DateRange(new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void DateRange_Contains_IncludesBothLimits()
    {
        var range = new DateRange(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));

        Assert.True(range.Contains(new DateOnly(2026, 11, 1)));
        Assert.True(range.Contains(new DateOnly(2026, 11, 10)));
        Assert.False(range.Contains(new DateOnly(2026, 11, 11)));
    }

    [Fact]
    public void Evaluator_ReturnsOnlySatisfiedRulesOfTheRequestedKind()
    {
        var evaluator = new BadgeEvaluator(new IBadgeRule[]
        {
            new FakeRule("a", BadgeKind.Achievement, true),
            new FakeRule("b", BadgeKind.Achievement, false),
            new FakeRule("c", BadgeKind.Shame, true)
        });

        IReadOnlyList<IBadgeRule> satisfied = evaluator.GetSatisfiedRules(Context(), BadgeKind.Achievement);

        Assert.Single(satisfied);
        Assert.Equal("a", satisfied[0].Code);
    }
}
