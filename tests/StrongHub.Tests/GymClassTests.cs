using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class GymClassTests
{
    [Fact]
    public void EachSubclass_DeclaresItsOwnTrainingFocus()
    {
        GymClass[] all =
        {
            new YogaClass(), new OpenGymClass(), new CrossfitClass(), new ZumbaClass(), new SpinningClass()
        };

        Assert.All(all, gymClass => Assert.False(string.IsNullOrWhiteSpace(gymClass.TrainingFocus)));
        Assert.Equal(5, all.Select(gymClass => gymClass.TrainingFocus).Distinct().Count());
    }
}
