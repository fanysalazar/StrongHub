namespace StrongHub.Models;

/// <summary>Everything a badge rule needs to decide if a member deserves a badge.</summary>
public class BadgeContext
{
    public BadgeContext(
        BasicUser member,
        IReadOnlyList<Measurement> measurements,
        IReadOnlyList<Visit> visits,
        DateOnly today,
        DateRange? range,
        IReadOnlySet<int> lowestVisitorIds)
    {
        Member = member;
        Measurements = measurements;
        Visits = visits;
        Today = today;
        Range = range;
        LowestVisitorIds = lowestVisitorIds;
    }

    public BasicUser Member { get; }
    public IReadOnlyList<Measurement> Measurements { get; }
    public IReadOnlyList<Visit> Visits { get; }
    public DateOnly Today { get; }

    /// <summary>Period chosen for shame badges (null for achievement badges).</summary>
    public DateRange? Range { get; }

    /// <summary>Ids of the members with the fewest visits in the period.</summary>
    public IReadOnlySet<int> LowestVisitorIds { get; }

    /// <summary>Measurements inside the range (or all of them), oldest first.</summary>
    public IReadOnlyList<Measurement> MeasurementsIn(DateRange? range = null) =>
        Measurements
            .Where(measurement => range is null || range.Contains(measurement.Date))
            .OrderBy(measurement => measurement.Date)
            .ThenBy(measurement => measurement.Id)
            .ToList();

    public Measurement? LatestMeasurement(DateRange? range = null) => MeasurementsIn(range).LastOrDefault();

    public int VisitsIn(DateRange range) => Visits.Count(visit => range.Contains(visit.Date));

    public DateRange RequireRange() =>
        Range ?? throw new InvalidOperationException("This badge rule needs a date range.");
}
