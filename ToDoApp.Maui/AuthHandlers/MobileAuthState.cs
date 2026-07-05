using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Maui.AuthHandlers;

public class MobileAuthState : IAuthState
{
    private bool _isLoggingOut = false;
    public SemaphoreSlim RefreshLock { get; } = new(1, 1);
    public void ResetLogoutState() => _isLoggingOut = false;
    public void SetLoggingOut() => _isLoggingOut = true;

    public bool IsLoggingOut()
    {
        return _isLoggingOut;
    }
}
