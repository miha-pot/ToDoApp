using System.Text.Json;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Maui.Services;

public class MobileDataStorage : IDataStorage
{
    public bool PersistsRefreshToken => true;

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var value = await SecureStorage.GetAsync(key);
        if (value is null) return default;
        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetItemAsync<T>(string key, T value)
        => await SecureStorage.SetAsync(key, JsonSerializer.Serialize(value));

    public Task RemoveItemAsync(string key)
    {
        SecureStorage.Remove(key);
        return Task.CompletedTask;
    }
}