using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the trainers table shown in the view.</summary>
public record TrainerRow(int Id, string Name, string Username, string Type);

/// <summary>Lists trainers and assigns a trainer to the logged-in member.</summary>
public class TrainerController
{
    private readonly IRepository<Trainer> _trainers;
    private readonly IRepository<BasicUser> _users;
    private readonly SessionContext _session;

    public TrainerController(IRepository<Trainer> trainers, IRepository<BasicUser> users, SessionContext session)
    {
        _trainers = trainers;
        _users = users;
        _session = session;
    }

    /// <summary>Only members can have a trainer assigned.</summary>
    public bool CanAssign => _session.CurrentUser is BasicUser;

    /// <summary>Trainers sorted by name, optionally filtered by type.</summary>
    public IReadOnlyList<TrainerRow> GetTrainerRows(TrainerType? type = null) =>
        _trainers.GetAll()
            .Where(trainer => type is null || trainer.Type == type)
            .OrderBy(trainer => trainer.Name)
            .Select(trainer => new TrainerRow(trainer.Id, trainer.Name, trainer.Username, trainer.Type.ToString()))
            .ToList();

    /// <summary>Text describing the trainer of the current member.</summary>
    public string GetAssignedTrainerText()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Trainer view (read only).";
        }

        Trainer? trainer = member.TrainerId is int id ? _trainers.GetById(id) : null;
        return trainer is null
            ? "You have no trainer assigned."
            : $"Your trainer: {trainer.Name} ({trainer.Type})";
    }

    /// <summary>Assigns the trainer to the current member and saves it.</summary>
    public void AssignTrainer(int trainerId)
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            throw new ValidationException("Only members can have a trainer assigned.");
        }

        if (_trainers.GetById(trainerId) is null)
        {
            throw new ValidationException("The selected trainer does not exist.");
        }

        member.TrainerId = trainerId;
        _users.Update(member);
    }
}
