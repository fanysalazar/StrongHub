namespace StrongHub.Data;

/// <summary>Where the number of days used by the trainer alerts is stored.</summary>
public interface IAlertSettingsStore
{
    int LoadThresholdDays();

    void SaveThresholdDays(int days);
}
