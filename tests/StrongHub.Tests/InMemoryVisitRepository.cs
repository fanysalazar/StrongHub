using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Tests;

/// <summary>Fake visit repository used by controller tests.</summary>
public class InMemoryVisitRepository : InMemoryRepository<Visit>, IVisitRepository
{
    public IReadOnlyList<Visit> GetByUser(int userId) =>
        GetAll()
            .Where(visit => visit.UserId == userId)
            .OrderByDescending(visit => visit.Date)
            .ThenByDescending(visit => visit.Id)
            .ToList();
}
