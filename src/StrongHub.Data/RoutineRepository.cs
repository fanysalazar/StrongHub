using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Stores routines in a JSON file.</summary>
public class RoutineRepository : JsonRepository<Routine>, IRoutineRepository
{
    public RoutineRepository(string filePath) : base(filePath)
    {
    }

    public IReadOnlyList<Routine> GetByUser(int userId) =>
        GetAll()
            .Where(routine => routine.UserId == userId)
            .OrderByDescending(routine => routine.StartDate)
            .ToList();
}
