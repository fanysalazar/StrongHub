using StrongHub.Data;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class JsonRepositoryTests : IDisposable
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
    public void Add_PersistsItemAndAssignsId()
    {
        File.WriteAllText(_path, "[]");
        var repository = new JsonRepository<Trainer>(_path);

        repository.Add(new Trainer { Name = "Ana", Username = "ana", Type = TrainerType.Gym });

        var reloaded = new JsonRepository<Trainer>(_path);
        Assert.Single(reloaded.GetAll());
        Assert.Equal(1, reloaded.GetAll()[0].Id);
        Assert.Equal(TrainerType.Gym, reloaded.GetAll()[0].Type);
    }

    [Fact]
    public void Constructor_MissingFile_ThrowsDataAccessException()
    {
        Assert.Throws<DataAccessException>(() => new JsonRepository<Trainer>(_path));
    }

    [Fact]
    public void Constructor_InvalidJson_ThrowsDataAccessException()
    {
        File.WriteAllText(_path, "this is not json");
        Assert.Throws<DataAccessException>(() => new JsonRepository<Trainer>(_path));
    }

    [Fact]
    public void Delete_UnknownId_ThrowsDataAccessException()
    {
        File.WriteAllText(_path, "[]");
        var repository = new JsonRepository<Trainer>(_path);
        Assert.Throws<DataAccessException>(() => repository.Delete(99));
    }
}
