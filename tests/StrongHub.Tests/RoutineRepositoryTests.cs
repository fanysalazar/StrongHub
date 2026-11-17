using StrongHub.Data;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineRepositoryTests : IDisposable
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
    public void Load_RoutineJson_ReadsDatesAndExercises()
    {
        File.WriteAllText(_path, """
        [
          {
            "id": 1, "userId": 4, "trainerId": 2, "name": "Block A",
            "startDate": "2026-10-01", "endDate": "2026-10-29",
            "exercises": [ { "name": "Deadlift", "sets": 3, "reps": 5, "weightKg": 80 } ]
          }
        ]
        """);

        var repository = new RoutineRepository(_path);

        Routine routine = repository.GetAll()[0];
        Assert.Equal(new DateOnly(2026, 10, 1), routine.StartDate);
        Assert.Equal(80, routine.Exercises[0].WeightKg);
    }

    [Fact]
    public void GetByUser_ReturnsOnlyThatMemberNewestFirst()
    {
        File.WriteAllText(_path, "[]");
        var repository = new RoutineRepository(_path);
        repository.Add(new Routine { UserId = 4, Name = "Older", StartDate = new DateOnly(2026, 9, 1) });
        repository.Add(new Routine { UserId = 4, Name = "Newer", StartDate = new DateOnly(2026, 10, 1) });
        repository.Add(new Routine { UserId = 5, Name = "Other member", StartDate = new DateOnly(2026, 10, 5) });

        IReadOnlyList<Routine> routines = repository.GetByUser(4);

        Assert.Equal(2, routines.Count);
        Assert.Equal("Newer", routines[0].Name);
    }
}
