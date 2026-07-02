using System.Runtime.Versioning;
using System.Text.Json;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Maui.Services;

public class MauiStorageService : IDataStorage
{
    [SupportedOSPlatform("MacCatalyst15.0")] // 🟢 Pove prevajalniku, da je varno za Mac 15+
    [SupportedOSPlatform("Android")]         // Podprto na vseh različicah Androida
    [SupportedOSPlatform("iOS")]             // Podprto na vseh različicah iOS
    [SupportedOSPlatform("Windows")]         // Podprto na Windowsih
    public ValueTask<T?> GetItemAsync<T>(string key)
    {
        string? json = Preferences.Default.Get<string?>(key, null);

        if (json == null)
        {
            return ValueTask.FromResult<T?>(default);
        }

        var result = JsonSerializer.Deserialize<T>(json);
        return ValueTask.FromResult(result);
    }

    [SupportedOSPlatform("MacCatalyst15.0")]
    public ValueTask SetItemAsync<T>(string key, T value)
    {
        string json = JsonSerializer.Serialize(value);

        Preferences.Default.Set(key, json);

        return ValueTask.CompletedTask;
    }

    [SupportedOSPlatform("MacCatalyst15.0")]
    public ValueTask RemoveItemAsync(string key)
    {
        Preferences.Default.Remove(key);

        return ValueTask.CompletedTask;
    }
}
