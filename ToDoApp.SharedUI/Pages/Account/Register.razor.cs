using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.Common;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.Account;

public partial class Register : IDisposable
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthStateTask { get; set; } = null!;

    [Inject]
    public required IAuthService AuthService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }


    private readonly CancellationTokenSource _cts = new();

    private MudForm _form = null!;

    private RegisterRequest model = new();
    private RegisterRequestValidator _validator = new();

    private bool _isSubmitting;
    private bool _isCheckingAuth = true;

    private ApiResponse<bool> _apiResponse = ApiResponse<bool>.Empty();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateTask;
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            NavigationManager.NavigateTo("/todo", forceLoad: false);
        }
        else
        {
            _isCheckingAuth = false;
        }
    }

    private async Task HandleRegistration()
    {
        _apiResponse = ApiResponse<bool>.Empty();

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            Snackbar.Add("Form not valid! Check fields.", MudBlazor.Severity.Error);
            return;
        }

        _isSubmitting = true;

        var result = await AuthService.RegisterAsync(model, _cts.Token);
        _apiResponse = result;
        if (result.IsSuccess)
        {
            Snackbar.Add("User profile was created! Redirecting...", MudBlazor.Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Snackbar.Add("Error while creating profile! Check fields..", MudBlazor.Severity.Error);
        }

        _isSubmitting = false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
