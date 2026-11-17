namespace StrongHub.Models;

/// <summary>Thrown when a routine id does not exist (for example, it was already deleted).</summary>
public class RoutineNotFoundException : Exception
{
    public RoutineNotFoundException(int routineId)
        : base($"Routine {routineId} was not found. It may have been deleted already.")
    {
    }
}
