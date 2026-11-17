using StrongHub.Data;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class VisitRepositoryTests : IDisposable
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
    public void Load_VisitJson_ReadsClassAndDate()
    {
        File.WriteAllText(_path, """
        [ { "id": 1, "userId": 3, "gymClass": "Zumba", "date": "2026-11-10" } ]
        """);

        var repository = new VisitRepository(_path);

        Visit visit = repository.GetAll()[0];
        Assert.Equal("Zumba", visit.ClassName);
        Assert.Equal(new DateOnly(2026, 11, 10), visit.Date);
    }

    [Fact]
    public void GetByUser_ReturnsOnlyThatMemberNewestFirst()
    {
        File.WriteAllText(_path, "[]");
        var repository = new VisitRepository(_path);
        repository.Add(new Visit { UserId = 3, ClassName = "Yoga", Date = new DateOnly(2026, 11, 2) });
        repository.Add(new Visit { UserId = 3, ClassName = "Zumba", Date = new DateOnly(2026, 11, 9) });
        repository.Add(new Visit { UserId = 4, ClassName = "Yoga", Date = new DateOnly(2026, 11, 12) });

        IReadOnlyList<Visit> visits = repository.GetByUser(3);

        Assert.Equal(2, visits.Count);
        Assert.Equal("Zumba", visits[0].ClassName);
    }
}
