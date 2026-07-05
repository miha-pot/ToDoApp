namespace ToDoApp.SharedUI.ServiceContracts;

public interface IDataStorage
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    bool PersistsRefreshToken { get; }
}
