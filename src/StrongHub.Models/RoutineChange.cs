namespace StrongHub.Models;

/// <summary>One recorded change of the weight of an exercise inside a routine.</summary>
public class RoutineChange : IEntity
{
    public int Id { get; set; }
    public int RoutineId { get; set; }
    public int UserId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public double OldWeightKg { get; set; }
    public double NewWeightKg { get; set; }
}
