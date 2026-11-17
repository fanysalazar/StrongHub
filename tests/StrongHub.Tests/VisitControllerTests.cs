using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class VisitControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateOnly Monday = new(2026, 11, 16);
    private static readonly DateOnly Tuesday = new(2026, 11, 17);

    private readonly InMemoryVisitRepository _visits = new();
    private readonly InMemoryRepository<GymClass> _classes = new();
    private readonly SessionContext _session = new();
    private readonly VisitController _controller;

    public VisitControllerTests()
    {
        _classes.Add(new YogaClass { Name = "Yoga" });
        _classes.Add(new ZumbaClass { Name = "Zumba" });
        _classes.Add(new SpinningClass { Name = "Spinning" });
        _classes.Add(new CrossfitClass { Name = "Crossfit" });

        _session.SignIn(new BasicUser { Id = 1, Name = "Nazareth Quiros", Username = "nazareth" });
        _controller = new VisitController(_visits, _classes, _session, () => Today);
    }

    private void RegisterFiveVisitsOfTheWeek()
    {
        _controller.Register("Yoga", Monday);
        _controller.Register("Yoga", Tuesday);
        _controller.Register("Yoga", Today);
        _controller.Register("Zumba", Today);
    }

    [Fact]
    public void Register_ValidVisit_SavesItWithTheCatalogName()
    {
        VisitResult result = _controller.Register("Yoga", Today);

        Assert.Equal(1, result.VisitsThisWeek);
        Assert.False(result.RestRecommended);
        Assert.Single(_visits.GetAll());
        Assert.Equal(1, _visits.GetAll()[0].UserId);
    }

    [Fact]
    public void Register_ClassNameIgnoresCase()
    {
        _controller.Register("  yOgA ", Today);

        Assert.Equal("Yoga", _visits.GetAll()[0].ClassName);
    }

    [Fact]
    public void Register_FutureDate_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.Register("Yoga", Today.AddDays(1)));
    }

    [Fact]
    public void Register_UnknownClass_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.Register("Pilates", Today));
    }

    [Fact]
    public void Register_SameClassSameDay_ThrowsValidationException()
    {
        _controller.Register("Yoga", Today);

        Assert.Throws<ValidationException>(() => _controller.Register("Yoga", Today));
    }

    [Fact]
    public void Register_AsTrainer_ThrowsValidationException()
    {
        _session.SignIn(new Trainer { Id = 3, Name = "Greivin Segura", Username = "greivin" });

        Assert.False(_controller.CanRegister);
        Assert.Throws<ValidationException>(() => _controller.Register("Yoga", Today));
    }

    [Fact]
    public void Register_FifthVisitInTheWeek_RecommendsRestAndRaisesTheEvent()
    {
        WeeklyRestEventArgs? raised = null;
        _controller.RestRecommended += (_, args) => raised = args;
        RegisterFiveVisitsOfTheWeek();
        Assert.Null(raised);

        VisitResult fifth = _controller.Register("Spinning", Today);

        Assert.True(fifth.RestRecommended);
        Assert.Equal(5, fifth.VisitsThisWeek);
        Assert.NotNull(raised);
        Assert.Equal("Nazareth Quiros", raised!.MemberName);
        Assert.Equal(Monday, raised.WeekStart);
        Assert.Equal(5, raised.Visits);
    }

    [Fact]
    public void Register_FourthVisit_DoesNotRecommendRest()
    {
        _controller.Register("Yoga", Monday);
        _controller.Register("Yoga", Tuesday);
        _controller.Register("Yoga", Today);

        VisitResult fourth = _controller.Register("Zumba", Today);

        Assert.False(fourth.RestRecommended);
        Assert.Equal(4, fourth.VisitsThisWeek);
    }

    [Fact]
    public void Register_SixthVisit_DoesNotRaiseTheEventAgain()
    {
        int timesRaised = 0;
        _controller.RestRecommended += (_, _) => timesRaised++;
        RegisterFiveVisitsOfTheWeek();
        _controller.Register("Spinning", Today);

        VisitResult sixth = _controller.Register("Crossfit", Today);

        Assert.False(sixth.RestRecommended);
        Assert.Equal(6, sixth.VisitsThisWeek);
        Assert.Equal(1, timesRaised);
    }

    [Fact]
    public void Register_VisitOfThePreviousWeek_IsNotCounted()
    {
        _controller.Register("Yoga", new DateOnly(2026, 11, 15));

        VisitResult result = _controller.Register("Yoga", Monday);

        Assert.Equal(1, result.VisitsThisWeek);
    }

    [Fact]
    public void GetHistoryRows_ReturnsOnlyOwnVisitsNewestFirst()
    {
        _visits.Add(new Visit { UserId = 2, ClassName = "Yoga", Date = Today });
        _controller.Register("Yoga", Monday);
        _controller.Register("Zumba", Tuesday);

        IReadOnlyList<VisitRow> rows = _controller.GetHistoryRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal("2026-11-17", rows[0].Date);
        Assert.Equal("Zumba", rows[0].ClassName);
    }

    [Fact]
    public void GetWeekSummary_Member_ShowsTheCountOfTheCurrentWeek()
    {
        _controller.Register("Yoga", Monday);
        _controller.Register("Yoga", Tuesday);

        Assert.Equal("Visits this week: 2 (rest day recommended at 5)", _controller.GetWeekSummary());
    }
}
