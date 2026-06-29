namespace ToDoApp.SharedUI.ServiceContracts;

public interface IDataStorage
{
    ValueTask<T?> GetItemAsync<T>(string key);
    ValueTask SetItemAsync<T>(string key, T value);
    ValueTask RemoveItemAsync(string key);
}
