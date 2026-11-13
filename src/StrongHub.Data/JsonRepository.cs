using System.Text.Json;
using System.Text.Json.Serialization;
using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Generic repository that stores its items in a JSON file.</summary>
public class JsonRepository<T> : IRepository<T> where T : class, IEntity
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private readonly string _filePath;
    private readonly List<T> _items;

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
        _items = Load();
    }

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public T? GetById(int id) => _items.FirstOrDefault(item => item.Id == id);

    public void Add(T item)
    {
        item.Id = _items.Count == 0 ? 1 : _items.Max(existing => existing.Id) + 1;
        _items.Add(item);
        Save();
    }

    public void Update(T item)
    {
        int index = _items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0)
        {
            throw new DataAccessException($"Item {item.Id} was not found.");
        }

        _items[index] = item;
        Save();
    }

    public void Delete(int id)
    {
        if (_items.RemoveAll(item => item.Id == id) == 0)
        {
            throw new DataAccessException($"Item {id} was not found.");
        }

        Save();
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private List<T> Load()
    {
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new DataAccessException($"Data file not found: {_filePath}", ex);
        }
        catch (JsonException ex)
        {
            throw new DataAccessException($"Invalid JSON in file: {_filePath}", ex);
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, Options));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataAccessException($"Could not write file: {_filePath}", ex);
        }
    }
}
