namespace StrongHub.Models;

/// <summary>A gym member who follows routines and tracks progress.</summary>
public class BasicUser : User
{
    public int Age { get; set; }
    public double WeightKg { get; set; }
    public double HeightM { get; set; }
    public double Bmi { get; set; }
    public int? TrainerId { get; set; }
    public double GoalBodyFatPercent { get; set; }
    public double GoalMuscleKg { get; set; }
}
