using System.Globalization;
using System.Resources;
using ToDoApp.SharedUI.Resources;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.States;

public class LanguageState
{
    private static readonly ResourceManager _resourceManager =
        new ResourceManager(typeof(AppStrings));

    private const string StorageKey = "app_culture";
    private readonly IDataStorage _storage;
    private string _currentCultureCode = "sl";

    public string CurrentLanguage => _currentCultureCode;
    public event Action? OnLanguageChanged;

    public LanguageState(IDataStorage storage)
    {
        _storage = storage;
    }

    public string T(string key)
    {
        try
        {
            var culture = new CultureInfo(_currentCultureCode);
            return _resourceManager.GetString(key, culture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedCulture = await _storage.GetItemAsync<string>(StorageKey);
            if (!string.IsNullOrEmpty(savedCulture))
                SetCulture(savedCulture);
        }
        catch { }
    }

    public async Task SetLanguageAsync(string cultureCode)
    {
        SetCulture(cultureCode);

        try
        {
            await _storage.SetItemAsync(StorageKey, cultureCode);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    private void SetCulture(string cultureCode)
    {
        _currentCultureCode = cultureCode;

        var culture = new CultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}