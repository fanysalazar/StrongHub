namespace StrongHub.Models;

/// <summary>One exercise inside a routine.</summary>
public class Exercise
{
    public string Name { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double WeightKg { get; set; }
}

/// <summary>A workout plan that a trainer creates for a member.</summary>
public class Routine : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TrainerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<Exercise> Exercises { get; set; } = new();
}
