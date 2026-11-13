namespace StrongHub.Models;

public enum TrainerType
{
    Personal,
    Gym
}

/// <summary>A personal or gym trainer.</summary>
public class Trainer : User
{
    public TrainerType Type { get; set; }
}
