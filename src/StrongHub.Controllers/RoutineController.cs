using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>A member that can be chosen in the routine form.</summary>
public record MemberOption(int Id, string Label);

/// <summary>One row of the routine list shown in the view.</summary>
public record RoutineRow(int Id, string Member, string Name, string Start, string End, int Exercises);

/// <summary>Creates, lists, edits and deletes routines. Only trainers can change them.</summary>
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
        int? trainerId = CurrentTrainerId;

        return _users.GetAll()
            .OrderByDescending(member => IsOwnMember(member, trainerId))
            .ThenBy(member => member.Name)
            .Select(member => new MemberOption(member.Id, BuildLabel(member, IsOwnMember(member, trainerId))))
            .ToList();
    }

    public int CountRoutinesOf(int memberId) => _routines.GetByUser(memberId).Count;

    /// <summary>Routines for the list, newest first. Can filter by member and by the current trainer.</summary>
    public IReadOnlyList<RoutineRow> GetRoutineRows(int? memberId = null, bool onlyMine = false)
    {
        Dictionary<int, string> memberNames = _users.GetAll().ToDictionary(member => member.Id, member => member.Name);
        int? trainerId = CurrentTrainerId;

        return _routines.GetAll()
            .Where(routine => memberId is null || routine.UserId == memberId)
            .Where(routine => !onlyMine || routine.TrainerId == trainerId)
            .OrderByDescending(routine => routine.StartDate)
            .ThenByDescending(routine => routine.Id)
            .Select(routine => new RoutineRow(
                routine.Id,
                memberNames.GetValueOrDefault(routine.UserId, "Unknown member"),
                routine.Name,
                routine.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                routine.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                routine.Exercises.Count))
            .ToList();
    }

    /// <summary>A copy of the routine for the edit form, so cancelling changes nothing.</summary>
    public Routine GetRoutineForEdit(int routineId)
    {
        Routine routine = _routines.GetById(routineId) ?? throw new RoutineNotFoundException(routineId);

        return new Routine
        {
            Id = routine.Id,
            UserId = routine.UserId,
            TrainerId = routine.TrainerId,
            Name = routine.Name,
            StartDate = routine.StartDate,
            EndDate = routine.EndDate,
            Exercises = CloneExercises(routine.Exercises)
        };
    }

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
        Trainer trainer = RequireTrainer();

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
            Exercises = CloneExercises(exercises)
        };

        _routines.Add(routine);
        RoutineCreated?.Invoke(this, new RoutineCreatedEventArgs(routine, member.Name, trainer.Name));
        return routine;
    }

    /// <summary>Validates and saves the changes of an existing routine. The member does not change.</summary>
    public Routine Update(int routineId, string name, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<Exercise> exercises)
    {
        RequireTrainer();

        Routine routine = _routines.GetById(routineId) ?? throw new RoutineNotFoundException(routineId);

        name = name.Trim();
        ValidateRoutine(name, startDate, endDate, exercises);

        routine.Name = name;
        routine.StartDate = startDate;
        routine.EndDate = endDate;
        routine.Exercises = CloneExercises(exercises);

        _routines.Update(routine);
        return routine;
    }

    /// <summary>Deletes a routine. The view must ask for confirmation before calling this.</summary>
    public void Delete(int routineId)
    {
        RequireTrainer();

        if (_routines.GetById(routineId) is null)
        {
            throw new RoutineNotFoundException(routineId);
        }

        _routines.Delete(routineId);
    }

    private int? CurrentTrainerId => _session.CurrentUser is Trainer trainer ? trainer.Id : null;

    private Trainer RequireTrainer() =>
        _session.CurrentUser as Trainer ?? throw new ValidationException("Only trainers can manage routines.");

    private static List<Exercise> CloneExercises(IEnumerable<Exercise> exercises) =>
        exercises
            .Select(exercise => new Exercise
            {
                Name = exercise.Name,
                Sets = exercise.Sets,
                Reps = exercise.Reps,
                WeightKg = exercise.WeightKg
            })
            .ToList();

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
