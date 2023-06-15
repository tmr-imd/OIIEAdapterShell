using System.Text.Json;

namespace Oiie.Settings;

public class SettingsService
{
    public async Task<T> LoadSettings<T>(string name)
    {
        using var reader = new StreamReader($"./Data/Settings/{name}.json");
        var json = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("json can't be empty");

        var result = JsonSerializer.Deserialize<T>(json);

        if (result is null) throw new InvalidOperationException($"Could not deserialise json to type {typeof(T).FullName}");

        return result;
    }

    public async Task SaveSettings<T>(T settings, string name)
    {
        using var writer = new StreamWriter($"./Data/Settings/{name}.json");

        var json = JsonSerializer.Serialize(settings);

        await writer.WriteAsync(json);
    }
}