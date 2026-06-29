namespace ToDoApp.Shared.AuthDTO.ResetPassword;

public class ResetPassRequest
{
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Email { get; set; }
}
