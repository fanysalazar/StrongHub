using StrongHub.Data;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class GymClassRepositoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    [Fact]
    public void Load_EachKind_BecomesItsSubclass()
    {
        File.WriteAllText(_path, """
        [
          { "kind": "yoga", "id": 1, "name": "Yoga", "description": "d", "durationMinutes": 60 },
          { "kind": "openGym", "id": 2, "name": "Open gym", "description": "d", "durationMinutes": 120 },
          { "kind": "crossfit", "id": 3, "name": "Crossfit", "description": "d", "durationMinutes": 45 },
          { "kind": "zumba", "id": 4, "name": "Zumba", "description": "d", "durationMinutes": 50 },
          { "kind": "spinning", "id": 5, "name": "Spinning", "description": "d", "durationMinutes": 45 }
        ]
        """);

        var repository = new JsonRepository<GymClass>(_path);

        Assert.IsType<YogaClass>(repository.GetAll()[0]);
        Assert.IsType<OpenGymClass>(repository.GetAll()[1]);
        Assert.IsType<CrossfitClass>(repository.GetAll()[2]);
        Assert.IsType<ZumbaClass>(repository.GetAll()[3]);
        Assert.IsType<SpinningClass>(repository.GetAll()[4]);
    }

    [Fact]
    public void Add_KeepsTheKindWhenSavedAndReloaded()
    {
        File.WriteAllText(_path, "[]");
        var repository = new JsonRepository<GymClass>(_path);

        repository.Add(new ZumbaClass { Name = "Zumba", Description = "d", DurationMinutes = 50 });

        var reloaded = new JsonRepository<GymClass>(_path);
        Assert.IsType<ZumbaClass>(reloaded.GetAll()[0]);
    }
}
