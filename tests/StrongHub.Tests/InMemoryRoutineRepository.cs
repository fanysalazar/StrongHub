using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Tests;

/// <summary>Fake routine repository used by controller tests.</summary>
public class InMemoryRoutineRepository : InMemoryRepository<Routine>, IRoutineRepository
{
    public IReadOnlyList<Routine> GetByUser(int userId) =>
        GetAll()
            .Where(routine => routine.UserId == userId)
            .OrderByDescending(routine => routine.StartDate)
            .ToList();
}
