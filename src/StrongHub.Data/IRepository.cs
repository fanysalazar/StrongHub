using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Abstracts data access so JSON can be replaced by another source.</summary>
public interface IRepository<T> where T : IEntity
{
    IReadOnlyList<T> GetAll();
    T? GetById(int id);
    void Add(T item);
    void Update(T item);
    void Delete(int id);
}
