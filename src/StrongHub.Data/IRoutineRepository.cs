using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Routine storage with the queries that routines need.</summary>
public interface IRoutineRepository : IRepository<Routine>
{
    /// <summary>Routines of one member, newest start date first.</summary>
    IReadOnlyList<Routine> GetByUser(int userId);
}
