using Microsoft.JSInterop;
using System.Text.Json;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Web.Services;

public class WebDataStorage : IDataStorage
{
    private readonly IJSRuntime _jsRuntime;

    public bool PersistsRefreshToken => false;

    public WebDataStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
