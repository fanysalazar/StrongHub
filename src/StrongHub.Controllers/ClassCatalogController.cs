using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the gym class catalog shown in the view.</summary>
public record CatalogEntry(int Id, string Name, int DurationMinutes, string TrainingFocus, string Description);

/// <summary>Provides the catalog of classes offered by the gym.</summary>
public class ClassCatalogController
{
    private readonly IRepository<GymClass> _classes;

    public ClassCatalogController(IRepository<GymClass> classes) => _classes = classes;

    /// <summary>All classes in catalog order.</summary>
    public IReadOnlyList<CatalogEntry> GetCatalog() =>
        _classes.GetAll()
            .OrderBy(gymClass => gymClass.Id)
            .Select(ToEntry)
            .ToList();

    /// <summary>One class by id, or null when it does not exist.</summary>
    public CatalogEntry? FindEntry(int id)
    {
        GymClass? gymClass = _classes.GetById(id);
        return gymClass is null ? null : ToEntry(gymClass);
    }

    private static CatalogEntry ToEntry(GymClass gymClass) =>
        new(gymClass.Id, gymClass.Name, gymClass.DurationMinutes, gymClass.TrainingFocus, gymClass.Description);
}
