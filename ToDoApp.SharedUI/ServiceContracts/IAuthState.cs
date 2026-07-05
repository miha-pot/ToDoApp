namespace ToDoApp.SharedUI.ServiceContracts;

public interface IAuthState
{
    bool IsLoggingOut();
    SemaphoreSlim RefreshLock { get; }
    void ResetLogoutState();
    void SetLoggingOut();
}
