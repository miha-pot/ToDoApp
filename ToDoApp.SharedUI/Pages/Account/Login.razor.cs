using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.Common;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.Account;

public partial class Login : IDisposable
{
    [Inject]
    public required IAuthService AuthService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState> AuthStateTask { get; set; } = null!;

    private MudForm _form = null!;
    private LoginRequest _model = new();
    private readonly LoginRequestValidator _validator = new();

    private bool _isSubmitting;
    private bool _isCheckingAuth = true;

    private bool _passwordShow;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private readonly CancellationTokenSource _cts = new();

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

    private async Task ClearFields()
    {
        _model = new LoginRequest();

        StateHasChanged();
    }

    private void TogglePasswordVisibility()
    {
        if (_passwordShow)
        {
            _passwordShow = false;
            _passwordInput = InputType.Password;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
        }
        else
        {
            _passwordShow = true;
            _passwordInput = InputType.Text;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
        }
    }

    private async Task HandleLogin()
    {
        _apiResponse = ApiResponse<bool>.Empty();

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            Snackbar.Add("Form not valid! Check fields!", MudBlazor.Severity.Error);
            return;
        }

        _isSubmitting = true;

        var response = await AuthService.LoginAsync(_model, _cts.Token);
        _apiResponse = response;

        if (response.IsSuccess)
        {
            NavigationManager.NavigateTo("/todo");
        }
        else
        {
            Snackbar.Add("Error while logging in!", MudBlazor.Severity.Error);
        }

        _isSubmitting = false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
