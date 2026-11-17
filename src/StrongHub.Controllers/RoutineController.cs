using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>A member that can be chosen in the routine form.</summary>
public record MemberOption(int Id, string Label);

/// <summary>Creates routines for members. Only trainers can do it.</summary>
public class RoutineController
{
    private const int MaxExercises = 20;

    private readonly IRoutineRepository _routines;
    private readonly IRepository<BasicUser> _users;
    private readonly SessionContext _session;

    public RoutineController(IRoutineRepository routines, IRepository<BasicUser> users, SessionContext session)
    {
        _routines = routines;
        _users = users;
        _session = session;
    }

    /// <summary>Raised after a routine is saved (the timeline will listen to it).</summary>
    public event EventHandler<RoutineCreatedEventArgs>? RoutineCreated;

    public bool CanCreate => _session.CurrentUser is Trainer;

    /// <summary>Members for the combo box: the trainer's own members first, then by name.</summary>
    public IReadOnlyList<MemberOption> GetMemberOptions()
    {
        int? trainerId = _session.CurrentUser is Trainer trainer ? trainer.Id : null;

        return _users.GetAll()
            .OrderByDescending(member => IsOwnMember(member, trainerId))
            .ThenBy(member => member.Name)
            .Select(member => new MemberOption(member.Id, BuildLabel(member, IsOwnMember(member, trainerId))))
            .ToList();
    }

    public int CountRoutinesOf(int memberId) => _routines.GetByUser(memberId).Count;

    /// <summary>Builds one exercise from raw form text. Throws ValidationException if invalid.</summary>
    public Exercise ParseExercise(string name, string setsText, string repsText, string weightText)
    {
        name = name.Trim();
        if (name.Length == 0)
        {
            throw new ValidationException("Exercise name is required.");
        }

        return new Exercise
        {
            Name = name,
            Sets = ParseWholeNumber(setsText, "Sets", 1, 20),
            Reps = ParseWholeNumber(repsText, "Reps", 1, 100),
            WeightKg = ParseWeight(weightText)
        };
    }

    /// <summary>Validates and saves a routine, then raises RoutineCreated.</summary>
    public Routine Create(int memberId, string name, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<Exercise> exercises)
    {
        if (_session.CurrentUser is not Trainer trainer)
        {
            throw new ValidationException("Only trainers can create routines.");
        }

        BasicUser member = _users.GetById(memberId)
                           ?? throw new ValidationException("The selected member does not exist.");

        name = name.Trim();
        ValidateRoutine(name, startDate, endDate, exercises);

        var routine = new Routine
        {
            UserId = member.Id,
            TrainerId = trainer.Id,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            Exercises = exercises.ToList()
        };

        _routines.Add(routine);
        RoutineCreated?.Invoke(this, new RoutineCreatedEventArgs(routine, member.Name, trainer.Name));
        return routine;
    }

    private static void ValidateRoutine(string name, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<Exercise> exercises)
    {
        if (name.Length < 3)
        {
            throw new ValidationException("Routine name must have at least 3 characters.");
        }

        if (startDate > endDate)
        {
            throw new ValidationException("The start date cannot be after the end date.");
        }

        if (exercises.Count == 0)
        {
            throw new ValidationException("Add at least one exercise.");
        }

        if (exercises.Count > MaxExercises)
        {
            throw new ValidationException($"A routine can have at most {MaxExercises} exercises.");
        }
    }

    private static bool IsOwnMember(BasicUser member, int? trainerId) =>
        trainerId.HasValue && member.TrainerId == trainerId;

    private static string BuildLabel(BasicUser member, bool isOwn) =>
        isOwn ? $"{member.Name} ({member.Username}) - your member" : $"{member.Name} ({member.Username})";

    private static int ParseWholeNumber(string text, string field, int min, int max)
    {
        if (!int.TryParse(text.Trim(), out int value) || value < min || value > max)
        {
            throw new ValidationException($"{field} must be a whole number between {min} and {max}.");
        }

        return value;
    }

    private static double ParseWeight(string text)
    {
        bool valid = double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float,
            CultureInfo.InvariantCulture, out double weight);

        if (!valid || weight < 0 || weight > 500)
        {
            throw new ValidationException("Weight must be a number between 0 and 500 (use 0 for bodyweight).");
        }

        return weight;
    }
}
