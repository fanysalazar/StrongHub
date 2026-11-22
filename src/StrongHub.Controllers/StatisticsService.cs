using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Calculates statistics when they are requested (nothing is stored).</summary>
public class StatisticsService
{
    private readonly IRepository<Measurement> _measurements;
    private readonly IRepository<BasicUser> _users;
    private readonly IVisitRepository _visits;

    public StatisticsService(
        IRepository<Measurement> measurements,
        IRepository<BasicUser> users,
        IVisitRepository visits)
    {
        _measurements = measurements;
        _users = users;
        _visits = visits;
    }

    public int MemberCount => _users.GetAll().Count;

    /// <summary>Change of weight, muscle and body fat between the first and last measurement of the period.</summary>
    public EvolutionResult GetEvolution(BasicUser member, DateRange range)
    {
        List<Measurement> inRange = _measurements.GetAll()
            .Where(measurement => measurement.UserId == member.Id && range.Contains(measurement.Date))
            .OrderBy(measurement => measurement.Date)
            .ThenBy(measurement => measurement.Id)
            .ToList();

        if (inRange.Count < 2)
        {
            string message = $"Not enough measurements in this period: at least 2 are needed, found {inRange.Count}.";
            return new EvolutionResult(message, Array.Empty<StatRow>());
        }

        Measurement first = inRange[0];
        Measurement last = inRange[^1];

        var rows = new List<StatRow>
        {
            new("Weight (kg)", Format(first.WeightKg), Format(last.WeightKg),
                Delta(last.WeightKg - first.WeightKg), "-", "-"),
            new("Muscle (kg)", Format(first.MuscleKg), Format(last.MuscleKg),
                Delta(last.MuscleKg - first.MuscleKg), Format(member.GoalMuscleKg),
                DescribeProgress(first.MuscleKg, last.MuscleKg, member.GoalMuscleKg, higherIsBetter: true, "kg")),
            new("Body fat (%)", Format(first.BodyFatPercent), Format(last.BodyFatPercent),
                Delta(last.BodyFatPercent - first.BodyFatPercent), Format(member.GoalBodyFatPercent),
                DescribeProgress(first.BodyFatPercent, last.BodyFatPercent, member.GoalBodyFatPercent, higherIsBetter: false, "%"))
        };

        string summary = $"{inRange.Count} measurements between {Day(first.Date)} and {Day(last.Date)}.";
        return new EvolutionResult(summary, rows);
    }

    public int CountVisits(int userId, DateRange range) =>
        _visits.GetByUser(userId).Count(visit => range.Contains(visit.Date));

    /// <summary>The members with the most visits (ties are broken by id).</summary>
    public IReadOnlyList<RankingRow> GetMostVisits(DateRange range, int count) =>
        ToRows(CountAll(range).OrderByDescending(tally => tally.Visits).ThenBy(tally => tally.Member.Id), count);

    /// <summary>The members with the fewest visits (ties are broken by id).</summary>
    public IReadOnlyList<RankingRow> GetFewestVisits(DateRange range, int count) =>
        ToRows(CountAll(range).OrderBy(tally => tally.Visits).ThenBy(tally => tally.Member.Id), count);

    /// <summary>1 + the number of members with more visits, so ties share the position.</summary>
    public int GetPosition(int userId, DateRange range)
    {
        int mine = CountVisits(userId, range);
        return 1 + CountAll(range).Count(tally => tally.Visits > mine);
    }

    private sealed record Tally(BasicUser Member, int Visits);

    private List<Tally> CountAll(DateRange range) =>
        _users.GetAll().Select(member => new Tally(member, CountVisits(member.Id, range))).ToList();

    private static List<RankingRow> ToRows(IEnumerable<Tally> ordered, int count) =>
        ordered
            .Take(count)
            .Select((tally, index) => new RankingRow(index + 1, tally.Member.Name, tally.Member.Username, tally.Visits))
            .ToList();

    private static string DescribeProgress(double start, double end, double goal, bool higherIsBetter, string unit)
    {
        bool reached = higherIsBetter ? end >= goal : end <= goal;
        if (reached)
        {
            return "Goal reached";
        }

        double needed = higherIsBetter ? goal - start : start - goal;
        double achieved = higherIsBetter ? end - start : start - end;
        double percent = needed <= 0 ? 0 : Math.Clamp(achieved / needed * 100, 0, 100);
        double remaining = Math.Abs(goal - end);

        return $"{percent.ToString("0", CultureInfo.InvariantCulture)}% of the way, {Format(remaining)} {unit} to go";
    }

    private static string Format(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Delta(double difference) =>
        (Math.Round(difference, 1) + 0.0).ToString("+0.#;-0.#;0", CultureInfo.InvariantCulture);

    private static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
