using Microsoft.AspNetCore.Components;
using MudBlazor;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.Tag;

public partial class Index
{
    [Inject]
    public required ITagService TagService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    private MudTable<TagResponse> _table = null!;

    private string _searchString = "";

    private CancellationTokenSource _cts = new();

    private async Task<TableData<TagResponse>> LoadServerData(TableState state, CancellationToken cancellationToken)
    {
        string? sortBy = state.SortLabel;
        bool sortDescending = state.SortDirection == SortDirection.Descending;

        QueryRequest queryRequest = new QueryRequest()
        {
            Page = state.Page + 1,
            PageSize = state.PageSize,
            SortBy = sortBy,
            Filters = GetQueryFilters()
        };

        var response = await TagService.GetItemsWithQuery(queryRequest, cancellationToken);

        if (response.IsSuccess && response.Value != null)
        {
            return new TableData<TagResponse>()
            {
                TotalItems = response.Value.TotalCount,
                Items = response.Value.Items
            };
        }
        else
        {
            Snackbar.Add($"Napaka pri osveževanju baze: {response.ErrorDetail}", MudBlazor.Severity.Error);

            return new TableData<TagResponse>() { TotalItems = 0, Items = Array.Empty<TagResponse>() };
        }
    }

    private void OnSearchChanged(string text)
    {
        _searchString = text;
        _table.ReloadServerData();
    }

    private List<QueryFilter> GetQueryFilters()
    {
        var filters = new List<QueryFilter>();

        if (!string.IsNullOrWhiteSpace(_searchString))
            filters.Add(new QueryFilter
            {
                Field = nameof(TagResponse.Name),
                Value = _searchString,
                Operator = QueryFilterOperator.Contains
            });

        return filters;
    }

    private async Task RequestDeleteAsync(Guid id, string title)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };

        bool? result = await DialogService.ShowMessageBoxAsync("Opozorilo",
                                                               $"Ali ste prepričani, da želite izbrisati opravilo: \"{title}\"?",
                                                               yesText: "Izbriši",
                                                               cancelText: "Prekliči",
                                                               options: options);

        if (result == true)
        {
            var response = await TagService.DeleteAsync(id, CancellationToken.None);

            if (response.IsSuccess)
            {
                Snackbar.Add("Opravilo je bilo uspešno izbrisano.", MudBlazor.Severity.Success);
                await _table.ReloadServerData();
            }
            else
            {
                Snackbar.Add($"Napaka pri brisanju: {response.ErrorDetail}", MudBlazor.Severity.Error);
            }
        }
    }

    private void NavigateToCreate() => NavigationManager.NavigateTo("/tag/create");

    private void NavigateToEdit(Guid id) => NavigationManager.NavigateTo($"/tag/edit/{id}");

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
