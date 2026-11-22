using StrongHub.Data;

namespace StrongHub.Tests;

/// <summary>Fake settings store used by the alert tests.</summary>
public sealed class FakeAlertSettingsStore : IAlertSettingsStore
{
    public int Days { get; set; } = 7;

    public int LoadThresholdDays() => Days;

    public void SaveThresholdDays(int days) => Days = days;
}
