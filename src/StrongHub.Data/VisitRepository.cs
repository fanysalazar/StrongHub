using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Stores visits in a JSON file.</summary>
public class VisitRepository : JsonRepository<Visit>, IVisitRepository
{
    public VisitRepository(string filePath) : base(filePath)
    {
    }

    public IReadOnlyList<Visit> GetByUser(int userId) =>
        GetAll()
            .Where(visit => visit.UserId == userId)
            .OrderByDescending(visit => visit.Date)
            .ThenByDescending(visit => visit.Id)
            .ToList();
}
