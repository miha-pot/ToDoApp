using Microsoft.AspNetCore.Components;
using MudBlazor;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.Enums;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.ToDo;

public partial class NewIndex : IDisposable
{
    [Inject]
    public required IToDoService ToDoService { get; set; }

    [Inject]
    public required ISnackbar SnackBar { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    private MudTable<ToDoResponse> _pendingTable = null!;
    private MudTable<ToDoResponse> _completedTable = null!;
    private int _activeTabIndex = 0;
    private readonly CancellationTokenSource _cts = new();
    private Priority? _selectedPriorityFilter = null;
    private readonly List<QueryFilter> _filters = [];

    private async Task OnPriorityFilterChanged(Priority? value)
    {
        string columnName = nameof(ToDoResponse.Level);
        _selectedPriorityFilter = value;

        var existingFilter = _filters.FirstOrDefault(x => x.Field == columnName);

        if (value is null)
        {
            if (existingFilter != null)
            {
                _filters.Remove(existingFilter);
            }
        }
        else
        {
            if (existingFilter != null)
            {
                existingFilter.Value = ((int)value).ToString();
            }
            else
            {
                _filters.Add(new QueryFilter
                {
                    Field = columnName,
                    Operator = QueryFilterOperator.Equals,
                    Value = ((int)value).ToString()
                });
            }
        }

        await ReloadBothTablesAsync();
    }

    private void OnSearchChanged(string text)
    {
        var filter = _filters.FirstOrDefault(x => x.Field == nameof(ToDoResponse.Title));

        if (filter is null)
        {
            _filters.Add(new()
            {
                Field = nameof(ToDoResponse.Title),
                Operator = QueryFilterOperator.Contains,
                Value = text
            });
        }
        else
        {
            _filters.Remove(filter);
            _filters.Add(new()
            {
                Field = nameof(ToDoResponse.Title),
                Operator = QueryFilterOperator.Contains,
                Value = text
            });
        }

        _ = ReloadBothTablesAsync();
    }

    private async Task<TableData<ToDoResponse>> LoadServerData(TableState state, bool isCompleted, CancellationToken cancellationToken)
    {
        // Skupni filtri (iskanje, prioriteta) + skrit filter glede na aktivni zavihek
        List<QueryFilter> filters = [.. _filters,
            new QueryFilter
            {
                Field = nameof(ToDoResponse.IsCompleted),
                Operator = QueryFilterOperator.Equals,
                Value = isCompleted.ToString()
            }
        ];

        QueryRequest queryRequest = new QueryRequest()
        {
            Page = state.Page + 1,
            PageSize = state.PageSize,
            SortBy = state.SortLabel,
            SortDescending = state.SortDirection == SortDirection.Descending,
            Filters = filters
        };

        var response = await ToDoService.GetItemsWithQuery(queryRequest, cancellationToken);

        if (response.IsSuccess && response.Value != null)
        {
            return new TableData<ToDoResponse>()
            {
                TotalItems = response.Value.TotalCount,
                Items = response.Value.Items
            };
        }
        else
        {
            SnackBar.Add($"Napaka pri osveževanju baze: {response.ErrorDetail}", MudBlazor.Severity.Error);

            return new TableData<ToDoResponse>() { TotalItems = 0, Items = Array.Empty<ToDoResponse>() };
        }
    }

    private Task<TableData<ToDoResponse>> LoadPendingData(TableState state, CancellationToken cancellationToken) =>
        LoadServerData(state, isCompleted: false, cancellationToken);

    private Task<TableData<ToDoResponse>> LoadCompletedData(TableState state, CancellationToken cancellationToken) =>
        LoadServerData(state, isCompleted: true, cancellationToken);

    private async Task ReloadBothTablesAsync()
    {
        if (_pendingTable != null)
        {
            await _pendingTable.ReloadServerData();
        }

        if (_completedTable != null)
        {
            await _completedTable.ReloadServerData();
        }
    }

    private async Task RequestCompleteItemAsync(Guid itemId, string title)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };

        bool? result = await DialogService.ShowMessageBoxAsync("Opozorilo",
                                                               $"Ali ste želite spremeniti status opravila: \"{title}\"?",
                                                               yesText: "Spremeni",
                                                               cancelText: "Prekliči",
                                                               options: options);

        if (result == true)
        {
            ApiResponse<bool> completeActionResponse = await ToDoService.ChangeCompletionStatus(itemId, _cts.Token);

            if (completeActionResponse.IsSuccess)
            {
                SnackBar.Add("Status opravila je bil uspešno spremenjen.", MudBlazor.Severity.Success);

                // Opravilo se preseli med zavihkoma, zato osvežimo obe tabeli
                await ReloadBothTablesAsync();
            }
            else
            {
                SnackBar.Add("Napaka pri spreminjanju statusa opravila: " + completeActionResponse.ErrorDetail, MudBlazor.Severity.Error);
            }
        }
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
            var response = await ToDoService.DeleteAsync(id, CancellationToken.None);

            if (response.IsSuccess)
            {
                SnackBar.Add("Opravilo je bilo uspešno izbrisano.", MudBlazor.Severity.Success);

                await ReloadBothTablesAsync();
            }
            else
            {
                SnackBar.Add($"Napaka pri brisanju: {response.ErrorDetail}", MudBlazor.Severity.Error);
            }
        }
    }

    private static Color GetMudChipColor(Priority level) => level switch
    {
        Priority.Low => Color.Success,
        Priority.Medium => Color.Primary,
        Priority.High => Color.Warning,
        Priority.VeryLow => Color.Info,
        Priority.Critical => Color.Error,
        _ => Color.Default
    };

    private void NavigateToCreate() => NavigationManager.NavigateTo("/todo/create");

    private void NavigateToEdit(Guid id) => NavigationManager.NavigateTo($"/todo/edit/{id}");

    private void NavigateToDetails(Guid id) => NavigationManager.NavigateTo($"/todo/details/{id}");

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}