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

    [Parameter]
    public string? PreviousLocation { get; set; }

    [Parameter]
    public Guid? ParentId { get; set; }

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
        var tagResult = await TagService.GetActiveTags(_cts.Token);

        if (tagResult.IsSuccess && tagResult.Value is not null)
        {
            _tagList = tagResult.Value;
        }
        else
        {
            Snackbar.Add($"{tagResult.ErrorTitle}: {tagResult.ErrorDetail}", MudBlazor.Severity.Warning);
        }

        ApiResponse<ToDoResponse> response = await ToDoService.GetByIdAsync(Id, _cts.Token);

        if (!response.IsSuccess || response.Value is null)
        {
            Snackbar.Add($"{response.ErrorTitle}: {response.ErrorDetail}", MudBlazor.Severity.Error);

            NavigationManager.NavigateTo(SetUrlToPreviousLocation());

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
            if (result.IsWarning)
            {
                Snackbar.Add("Podatki niso bili spremenjeni!", MudBlazor.Severity.Warning);
            }
            else
            {
                Snackbar.Add("Podatki so bili uspešno spremenjeni!", MudBlazor.Severity.Success);
            }

            NavigationManager.NavigateTo(SetUrlToPreviousLocation());
        }

        _isSubmitting = false;
    }

    private string GetMultiSelectionText(IReadOnlyList<string> selectedValues)
    {
        return selectedValues.Count == 0
            ? "Ni izbranih oznak"
            : $"Izbrano ({selectedValues.Count})";
    }

    private void Cancel() => NavigationManager.NavigateTo(SetUrlToPreviousLocation());


    private string SetUrlToPreviousLocation()
    {
        // 1. Če vemo, da imamo ParentId, pomeni, da smo urejali sub-task.
        // Vrniti se moramo na details glavnega taska.
        if (ParentId.HasValue)
        {
            return $"/todo/details/{ParentId.Value}";
        }

        // 2. Če imamo PreviousLocation eksplicitno podan (npr. "details")
        if (!string.IsNullOrEmpty(PreviousLocation))
        {
            if (PreviousLocation.Equals(nameof(Dashboard)))
            {
                return $"/todo/{PreviousLocation.ToLower()}";
            }

            return $"/todo/{PreviousLocation.ToLower()}/{Id}";
        }

        // 3. "Fallback" (varnostna mreža): Če gre karkoli narobe, 
        // ali pa če smo prišli iz seznama, vrni na osnovni index.
        return "/todo";
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
