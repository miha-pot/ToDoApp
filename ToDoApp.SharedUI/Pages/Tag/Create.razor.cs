using Microsoft.AspNetCore.Components;
using MudBlazor;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.Shared.TagDTO.Validators;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.Tag;

public partial class Create : IDisposable
{
    [Parameter]
    public Guid? ParentToDoId { get; set; }

    [Inject]
    public required ITagService TagService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private MudForm _form = null!;
    private readonly TagAddRequest _model = new();
    private readonly TagAddRequestValidator _validator = new();

    private bool _isSubmitting;

    private readonly CancellationTokenSource _cts = new();

    private ApiResponse<TagResponse> _response = ApiResponse<TagResponse>.Empty();

    private MudBlazor.Utilities.MudColor PickerColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.ColorHex))
                return new MudBlazor.Utilities.MudColor("#000000");

            return new MudBlazor.Utilities.MudColor(_model.ColorHex);
        }
        set
        {
            if (value != null)
            {
                _model.ColorHex = value.Value;
            }
        }
    }

    private MudBlazor.Utilities.MudColor BgPickerColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.ColorHex))
                return new MudBlazor.Utilities.MudColor("#E0E0E0");

            return new MudBlazor.Utilities.MudColor(_model.BgColorHex);
        }
        set
        {
            if (value != null)
            {
                _model.BgColorHex = value.Value;
            }
        }
    }

    private async Task HandleValidSubmit()
    {
        _response = ApiResponse<TagResponse>.Empty();

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            Snackbar.Add("Obrazec ni veljaven. Preverite vnešene podatke.", MudBlazor.Severity.Error);
            return;
        }

        _isSubmitting = true;

        var result = await TagService.CreateAsync(_model, _cts.Token);
        _response = result;

        if (result.IsSuccess)
        {
            NavigationManager.NavigateTo("/tag");
        }

        _isSubmitting = false;
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/tag");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
