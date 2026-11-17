using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Data sent to subscribers when a routine is created.</summary>
public class RoutineCreatedEventArgs : EventArgs
{
    public RoutineCreatedEventArgs(Routine routine, string memberName, string trainerName)
    {
        Routine = routine;
        MemberName = memberName;
        TrainerName = trainerName;
    }

    public Routine Routine { get; }
    public string MemberName { get; }
    public string TrainerName { get; }
}
