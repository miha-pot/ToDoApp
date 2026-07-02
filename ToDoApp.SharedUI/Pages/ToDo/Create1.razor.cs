using Microsoft.AspNetCore.Components;
using MudBlazor;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.Shared.ToDoDTO.Validators;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.ToDo;

public partial class Create1 : IDisposable
{
    [Parameter]
    public Guid? ParentToDoId { get; set; }

    [Inject]
    public required IToDoService ToDoService { get; set; }

    [Inject]
    public required ITagService TagService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private MudForm _form = null!;
    private readonly ToDoAddRequest _model = new();
    private readonly ToDoAddRequestValidator _validator = new();

    private bool _isSubmitting;

    private readonly CancellationTokenSource _cts = new();

    private ApiResponse<ToDoResponse> _response = ApiResponse<ToDoResponse>.Empty();

    private List<TagResponse> _tagList = [];

    private IReadOnlyCollection<TagResponse> SelectedTags { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        _model.ParentToDoId = ParentToDoId;
        _tagList = await TagService.GetActiveTags(_cts.Token);
    }

    private static string GetMultiSelectionText(IReadOnlyList<string> selectedValues)
    {
        return selectedValues.Count == 0 ?
            "Ni izbranih oznak" : $"Izbrano ({selectedValues.Count})";
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

        _model.TagIds = SelectedTags.Select(x => x.Id).ToList();

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            Snackbar.Add("Obrazec ni veljaven. Preverite vnešene podatke.", MudBlazor.Severity.Error);
            return;
        }

        _isSubmitting = true;

        var result = await ToDoService.CreateAsync(_model, _cts.Token);
        if (result.IsSuccess)
        {
            NavigationManager.NavigateTo("/todo");
        }

        _response = result;
        _isSubmitting = false;
    }

    private void Cancel() => NavigationManager.NavigateTo("/todo");

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
