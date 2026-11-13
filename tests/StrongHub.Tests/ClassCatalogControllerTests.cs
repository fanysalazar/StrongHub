using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class ClassCatalogControllerTests
{
    private readonly InMemoryRepository<GymClass> _classes = new();
    private readonly ClassCatalogController _controller;

    public ClassCatalogControllerTests()
    {
        _classes.Add(new SpinningClass { Name = "Spinning", Description = "bikes", DurationMinutes = 45 });
        _classes.Add(new YogaClass { Name = "Yoga", Description = "mats", DurationMinutes = 60 });
        _controller = new ClassCatalogController(_classes);
    }

    [Fact]
    public void GetCatalog_ReturnsOneEntryPerClassInIdOrder()
    {
        IReadOnlyList<CatalogEntry> catalog = _controller.GetCatalog();

        Assert.Equal(2, catalog.Count);
        Assert.Equal("Spinning", catalog[0].Name);
        Assert.Equal("Yoga", catalog[1].Name);
    }

    [Fact]
    public void FindEntry_KnownId_ReturnsDurationAndFocus()
    {
        CatalogEntry? entry = _controller.FindEntry(2);

        Assert.NotNull(entry);
        Assert.Equal(60, entry!.DurationMinutes);
        Assert.Equal("Flexibility, balance and breathing", entry.TrainingFocus);
    }

    [Fact]
    public void FindEntry_UnknownId_ReturnsNull()
    {
        Assert.Null(_controller.FindEntry(99));
    }
}
