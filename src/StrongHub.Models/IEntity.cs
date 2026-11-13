namespace StrongHub.Models;

/// <summary>Any object that can be stored in a repository.</summary>
public interface IEntity
{
    int Id { get; set; }
}
