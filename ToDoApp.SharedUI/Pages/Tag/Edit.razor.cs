using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Mappers;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.Shared.TagDTO.Validators;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.Tag;

public partial class Edit
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required ITagService TagService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private MudForm _form = null!;
    private TagUpdateRequest _model = new();
    private TagUpdateRequestValidator _validator = new();

    private MudColor PickerColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.ColorHex))
                return new MudColor("#594AE2");

            return new MudColor(_model.ColorHex);
        }
        set
        {
            if (value != null)
            {
                _model.ColorHex = value.Value;
            }
        }
    }

    private MudColor BgPickerColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.BgColorHex))
                return new MudColor("#594AE2");

            return new MudColor(_model.BgColorHex);
        }
        set
        {
            if (value != null)
            {
                _model.BgColorHex = value.Value;
            }
        }
    }

    private bool _isSubmitting;
    private readonly CancellationTokenSource _cts = new();
    private ApiResponse<TagResponse> _response = ApiResponse<TagResponse>.Empty();

    protected override async Task OnParametersSetAsync()
    {
        var model = await TagService.GetByIdAsync(Id, _cts.Token);

        if (model.Value is not null)
        {
            _model = model.Value.ToUpdateRequest();
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

        var result = await TagService.UpdateAsync(_model, _cts.Token);
        _response = result;

        if (result.IsSuccess)
        {
            if (result.IsWarning)
            {
                Snackbar.Add("Podatki niso bili spremenjeni!", MudBlazor.Severity.Warning);
            }
            else
            {
                Snackbar.Add("Podatki so bili uspešno spremenjeni!", MudBlazor.Severity.Success);
            }

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
