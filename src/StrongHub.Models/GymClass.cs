using System.Text.Json.Serialization;

namespace StrongHub.Models;

/// <summary>Base class of every class offered by the gym.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(YogaClass), "yoga")]
[JsonDerivedType(typeof(OpenGymClass), "openGym")]
[JsonDerivedType(typeof(CrossfitClass), "crossfit")]
[JsonDerivedType(typeof(ZumbaClass), "zumba")]
[JsonDerivedType(typeof(SpinningClass), "spinning")]
public abstract class GymClass : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    /// <summary>Main goal of the class; each subclass defines its own.</summary>
    [JsonIgnore]
    public abstract string TrainingFocus { get; }
}
