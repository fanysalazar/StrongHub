using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Visit storage with the queries that visits need.</summary>
public interface IVisitRepository : IRepository<Visit>
{
    /// <summary>Visits of one member, newest first.</summary>
    IReadOnlyList<Visit> GetByUser(int userId);
}
