using Microsoft.AspNetCore.Components;
using MudBlazor;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Mappers;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.Shared.ToDoDTO.Validators;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.ToDo;

public partial class Edit : IDisposable
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required IToDoService ToDoService { get; set; }

    [Inject]
    public required ITagService TagService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private MudForm _form = null!;
    private ToDoUpdateRequest _model = new();
    private ToDoUpdateRequestValidator _validator = new();

    private bool _isSubmitting;
    private bool _isLoaded = false;

    private readonly CancellationTokenSource _cts = new();

    private ApiResponse<ToDoResponse> _response = ApiResponse<ToDoResponse>.Empty();

    private List<TagResponse> _tagList = [];

    private IReadOnlyCollection<TagResponse> SelectedTags { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        ApiResponse<ToDoResponse> response = await ToDoService.GetByIdAsync(Id, _cts.Token);

        _tagList = await TagService.GetActiveTags(_cts.Token);

        if (!response.IsSuccess || response.Value is null)
        {
            Snackbar.Add($"{response.ErrorTitle}: {response.ErrorDetail}", MudBlazor.Severity.Error);
            NavigationManager.NavigateTo("/todo");

            return;
        }

        _model = response.Value.ToUpdateRequest();

        SelectedTags = _model.Tags;

        _isLoaded = true;
    }

    private void RemoveTag(TagResponse tag)
    {
        var list = SelectedTags.ToList();
        list.Remove(tag);
        SelectedTags = list;
    }

    private async Task HandleValidSubmit()
    {
        _response = ApiResponse<ToDoResponse>.Empty();

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            Snackbar.Add("Obrazec ni veljaven. Preverite vnešene podatke.", MudBlazor.Severity.Error);
            return;
        }

        _isSubmitting = true;

        _model.TagIds = SelectedTags.Select(x => x.Id).ToList();

        var result = await ToDoService.UpdateAsync(_model, _cts.Token);
        _response = result;

        if (result.IsSuccess)
        {
            NavigationManager.NavigateTo("/todo");
        }

        _isSubmitting = false;
    }

    private string GetMultiSelectionText(IReadOnlyList<string> selectedValues)
    {
        return selectedValues.Count == 0
            ? "Ni izbranih oznak"
            : $"Izbrano ({selectedValues.Count})";
    }

    private void Cancel() => NavigationManager.NavigateTo("/todo");


    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
