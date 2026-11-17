namespace StrongHub.Models;

/// <summary>Body measurement of a member on a given day.</summary>
public class Measurement : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public double WeightKg { get; set; }
    public double BodyFatPercent { get; set; }
    public double MuscleKg { get; set; }
}
