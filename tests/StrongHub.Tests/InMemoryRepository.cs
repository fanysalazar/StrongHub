using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Tests;

/// <summary>Fake repository used by controller tests (no files involved).</summary>
public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();

    public IReadOnlyList<T> GetAll() => _items.ToList();

    public T? GetById(int id) => _items.FirstOrDefault(item => item.Id == id);

    public void Add(T item)
    {
        item.Id = _items.Count + 1;
        _items.Add(item);
    }

    public void Update(T item)
    {
        int index = _items.FindIndex(existing => existing.Id == item.Id);
        _items[index] = item;
    }

    public void Delete(int id) => _items.RemoveAll(item => item.Id == id);
}
