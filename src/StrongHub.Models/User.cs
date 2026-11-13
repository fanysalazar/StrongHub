namespace StrongHub.Models;

/// <summary>Base class for every person who can log in.</summary>
public abstract class User : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
