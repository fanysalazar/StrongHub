using System.Text.Json;
using StrongHub.Models;

namespace StrongHub.Data;

/// <summary>Content of settings.json.</summary>
public sealed class AlertSettingsFile
{
    public int AlertThresholdDays { get; set; } = JsonAlertSettingsStore.DefaultThresholdDays;
}

/// <summary>Stores the alert threshold in a small JSON file. A missing file means "use the default".</summary>
public class JsonAlertSettingsStore : IAlertSettingsStore
{
    public const int DefaultThresholdDays = 7;

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _filePath;

    public JsonAlertSettingsStore(string filePath) => _filePath = filePath;

    public int LoadThresholdDays()
    {
        if (!File.Exists(_filePath))
        {
            return DefaultThresholdDays;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AlertSettingsFile>(json, Options)?.AlertThresholdDays
                   ?? DefaultThresholdDays;
        }
        catch (JsonException ex)
        {
            throw new DataAccessException($"Invalid JSON in file: {_filePath}", ex);
        }
        catch (IOException ex)
        {
            throw new DataAccessException($"Could not read file: {_filePath}", ex);
        }
    }

    public void SaveThresholdDays(int days)
    {
        try
        {
            string json = JsonSerializer.Serialize(new AlertSettingsFile { AlertThresholdDays = days }, Options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataAccessException($"Could not write file: {_filePath}", ex);
        }
    }
}
