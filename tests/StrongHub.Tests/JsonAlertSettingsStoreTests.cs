using StrongHub.Data;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class JsonAlertSettingsStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsTheDefault()
    {
        Assert.Equal(7, new JsonAlertSettingsStore(_path).LoadThresholdDays());
    }

    [Fact]
    public void Load_ReadsTheCamelCaseFile()
    {
        File.WriteAllText(_path, """{ "alertThresholdDays": 14 }""");

        Assert.Equal(14, new JsonAlertSettingsStore(_path).LoadThresholdDays());
    }

    [Fact]
    public void Save_ThenLoadWithANewStore_ReturnsTheSavedValue()
    {
        new JsonAlertSettingsStore(_path).SaveThresholdDays(21);

        Assert.Equal(21, new JsonAlertSettingsStore(_path).LoadThresholdDays());
    }

    [Fact]
    public void Load_InvalidJson_ThrowsDataAccessException()
    {
        File.WriteAllText(_path, "this is not json");

        Assert.Throws<DataAccessException>(() => new JsonAlertSettingsStore(_path).LoadThresholdDays());
    }
}
