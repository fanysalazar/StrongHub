namespace StrongHub.Models;

/// <summary>Guided yoga session.</summary>
public class YogaClass : GymClass
{
    public override string TrainingFocus => "Flexibility, balance and breathing";
}

/// <summary>Free access to the training floor without an instructor.</summary>
public class OpenGymClass : GymClass
{
    public override string TrainingFocus => "Self-directed strength and cardio";
}

/// <summary>Timed functional training circuits.</summary>
public class CrossfitClass : GymClass
{
    public override string TrainingFocus => "Functional strength and conditioning";
}

/// <summary>Dance-based cardio workout.</summary>
public class ZumbaClass : GymClass
{
    public override string TrainingFocus => "Rhythmic cardio";
}

/// <summary>Instructor-led indoor cycling.</summary>
public class SpinningClass : GymClass
{
    public override string TrainingFocus => "Cycling endurance";
}
