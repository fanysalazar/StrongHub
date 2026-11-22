using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Statistics for the logged-in member. Trainers cannot use them.</summary>
public class StatisticsController
{
    public const int RankingSize = 10;

    private readonly StatisticsService _service;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;

    public StatisticsController(StatisticsService service, SessionContext session, Func<DateOnly>? today = null)
    {
        _service = service;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    public bool CanView => _session.CurrentUser is BasicUser;

    public DateOnly Today => _today();

    /// <summary>Weight, muscle and body fat change plus goal progress. Throws ValidationException for an invalid period.</summary>
    public EvolutionResult GetEvolution(DateOnly start, DateOnly end)
    {
        BasicUser member = RequireMember();
        DateRange range = BuildRange(start, end);
        return _service.GetEvolution(member, range);
    }

    /// <summary>Top 10 with the most and the fewest visits, and the position of the member.</summary>
    public RankingResult GetRankings(DateOnly start, DateOnly end)
    {
        BasicUser member = RequireMember();
        DateRange range = BuildRange(start, end);

        int visits = _service.CountVisits(member.Id, range);
        int position = _service.GetPosition(member.Id, range);
        string summary =
            $"You attended {visits} class(es) in this period. Your position: {position} of {_service.MemberCount} members.";

        return new RankingResult(
            _service.GetMostVisits(range, RankingSize),
            _service.GetFewestVisits(range, RankingSize),
            summary);
    }

    private BasicUser RequireMember() =>
        _session.CurrentUser as BasicUser
        ?? throw new ValidationException("Statistics are only available for members.");

    private DateRange BuildRange(DateOnly start, DateOnly end)
    {
        var range = new DateRange(start, end);

        if (end > Today)
        {
            throw new ValidationException("The end date cannot be in the future.");
        }

        return range;
    }
}
