using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the measurements tab.</summary>
public record MeasurementRow(string Date, double WeightKg, double BodyFatPercent, double MuscleKg);

/// <summary>Provides the body measurements of the logged-in member.</summary>
public class MeasurementController
{
    private readonly IRepository<Measurement> _measurements;
    private readonly SessionContext _session;

    public MeasurementController(IRepository<Measurement> measurements, SessionContext session)
    {
        _measurements = measurements;
        _session = session;
    }

    /// <summary>Measurements of the current member, newest first. Trainers get an empty list.</summary>
    public IReadOnlyList<MeasurementRow> GetRows()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return Array.Empty<MeasurementRow>();
        }

        return OwnMeasurements(member.Id)
            .OrderByDescending(measurement => measurement.Date)
            .ThenByDescending(measurement => measurement.Id)
            .Select(measurement => new MeasurementRow(
                measurement.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                measurement.WeightKg,
                measurement.BodyFatPercent,
                measurement.MuscleKg))
            .ToList();
    }

    public string GetSummary()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Measurements are available for members.";
        }

        List<Measurement> ordered = OwnMeasurements(member.Id)
            .OrderBy(measurement => measurement.Date)
            .ThenBy(measurement => measurement.Id)
            .ToList();

        if (ordered.Count == 0)
        {
            return "No measurements yet.";
        }

        if (ordered.Count == 1)
        {
            return "1 measurement recorded.";
        }

        Measurement first = ordered[0];
        Measurement last = ordered[^1];
        string since = first.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return $"{ordered.Count} measurements. Change since {since}: "
               + $"weight {Delta(last.WeightKg - first.WeightKg)} kg, "
               + $"body fat {Delta(last.BodyFatPercent - first.BodyFatPercent)} %, "
               + $"muscle {Delta(last.MuscleKg - first.MuscleKg)} kg";
    }

    private List<Measurement> OwnMeasurements(int userId) =>
        _measurements.GetAll().Where(measurement => measurement.UserId == userId).ToList();

    private static string Delta(double difference) =>
        Math.Round(difference, 1).ToString("+0.#;-0.#;0", CultureInfo.InvariantCulture);
}
