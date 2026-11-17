using System.Text.Json.Serialization;

namespace StrongHub.Models;

/// <summary>One attendance of a member to a gym class.</summary>
public class Visit : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [JsonPropertyName("gymClass")]
    public string ClassName { get; set; } = string.Empty;

    public DateOnly Date { get; set; }
}
