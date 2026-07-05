namespace ToDoApp.Maui.Services;

public class MobileAuthEventService
{
    public event Func<Task>? OnSessionExpired;

    public async Task NotifySessionExpiredAsync()
    {
        if (OnSessionExpired is not null)
            await OnSessionExpired.Invoke();
    }
}
