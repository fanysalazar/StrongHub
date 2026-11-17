using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Builds the information that badge rules need for one member.</summary>
public class BadgeContextFactory
{
    private readonly IRepository<Measurement> _measurements;
    private readonly IVisitRepository _visits;

    public BadgeContextFactory(IRepository<Measurement> measurements, IVisitRepository visits)
    {
        _measurements = measurements;
        _visits = visits;
    }

    public BadgeContext Create(
        BasicUser member,
        DateOnly today,
        DateRange? range = null,
        IReadOnlySet<int>? lowestVisitorIds = null)
    {
        List<Measurement> measurements = _measurements.GetAll()
            .Where(measurement => measurement.UserId == member.Id)
            .OrderBy(measurement => measurement.Date)
            .ThenBy(measurement => measurement.Id)
            .ToList();

        return new BadgeContext(
            member, measurements, _visits.GetByUser(member.Id), today, range, lowestVisitorIds ?? new HashSet<int>());
    }
}
