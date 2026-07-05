using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Web.AuthHandlers;

public class WebAuthState : IAuthState
{
    private bool _isLoggingOut = false;

    public bool IsLoggingOut()
    {
        return _isLoggingOut;
    }
    public SemaphoreSlim RefreshLock { get; } = new(1, 1);
    public void ResetLogoutState() => _isLoggingOut = false;
    public void SetLoggingOut() => _isLoggingOut = true;

}
