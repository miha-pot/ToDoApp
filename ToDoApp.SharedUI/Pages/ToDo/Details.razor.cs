using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using ToDoApp.Shared.Enums;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Pages.ToDo
{
    [Authorize]
    public partial class Details
    {
        [Parameter] public Guid Id { get; set; }

        [Parameter] public string? PreviousLocation { get; set; }

        [Inject]
        public required IToDoService ToDoService { get; set; }

        [Inject]
        public required ISnackbar Snackbar { get; set; }

        private bool _isLoading = true;
        private bool _isAddingSubtask = false;
        private string _newSubtaskTitle = string.Empty;
        private ToDoResponse? _mainTodo;
        private List<ToDoResponse> _subtasks = new();
        private int _subtaskProgress = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadTodoDetailsAsync();
        }

        private async Task LoadTodoDetailsAsync()
        {
            try
            {
                _isLoading = true;
                // Klic tvojega obstoječega ToDo servisa za pridobitev ene same naloge po IDju
                var response = await ToDoService.GetByIdAsync(Id, CancellationToken.None);

                if (response.IsSuccess && response.Value != null)
                {
                    _mainTodo = response.Value;

                    var subTasksResponse = await ToDoService.GetSubTasks(Id, CancellationToken.None);

                    if (subTasksResponse.IsSuccess && subTasksResponse.Value is not null)
                    {
                        _subtasks = subTasksResponse.Value;
                    }

                    CalculateProgress();
                }
                else if (response.StatusCode != 401)
                {
                    Snackbar.Add("Napaka pri nalaganju opravila.", MudBlazor.Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading todo details: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task AddSubtaskAsync()
        {
            if (string.IsNullOrWhiteSpace(_newSubtaskTitle)) return;

            try
            {
                _isAddingSubtask = true;

                // 🟢 Tukaj pokličeš backend API za dodajanje subtaska
                // Npr: var result = await ToDoService.CreateSubtaskAsync(Id, _newSubtaskTitle);

                ToDoAddRequest addRequest = new()
                {
                    Title = _newSubtaskTitle,
                    ParentToDoId = Id
                };

                var result = await ToDoService.CreateAsync(addRequest, CancellationToken.None);

                _newSubtaskTitle = string.Empty;

                CalculateProgress();
                Snackbar.Add("Podopravilo uspešno dodano.", MudBlazor.Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Napaka pri dodajanju: {ex.Message}", MudBlazor.Severity.Error);
            }
            finally
            {
                _isAddingSubtask = false;
            }
        }

        private async Task ToggleSubtaskStatusAsync(ToDoResponse subtask, bool isChecked)
        {
            try
            {
                subtask.IsCompleted = isChecked;

                // 🟢 Tukaj pokličeš API za posodobitev statusa subtaska na zaledju
                await ToDoService.ChangeCompletionStatus(subtask.Id, CancellationToken.None);

                CalculateProgress();
                Snackbar.Add(isChecked ? "Podopravilo zaključeno." : "Podopravilo ponovno odprto.", MudBlazor.Severity.Info);
            }
            catch (Exception ex)
            {
                subtask.IsCompleted = !isChecked; // v primeru napake vrnemo prejšnje stanje
                Snackbar.Add($"Napaka pri posodabljanju statusa: {ex.Message}", MudBlazor.Severity.Error);
            }
        }

        private async Task DeleteSubtaskAsync(Guid subtaskId)
        {
            try
            {
                // 🟢 Tukaj pokličeš API za brisanje subtaska na zaledju
                await ToDoService.DeleteAsync(subtaskId, CancellationToken.None);

                CalculateProgress();
                Snackbar.Add("Podopravilo izbrisano.", MudBlazor.Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Napaka pri brisanju: {ex.Message}", MudBlazor.Severity.Error);
            }
        }

        private void CalculateProgress()
        {
            if (!_subtasks.Any())
            {
                _subtaskProgress = _mainTodo?.IsCompleted == true ? 100 : 0;
                return;
            }

            int completed = _subtasks.Count(x => x.IsCompleted);
            _subtaskProgress = (int)Math.Round((double)completed / _subtasks.Count * 100);
        }

        private async Task HandleQuickAddKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await AddSubtaskAsync();
            }
        }

        private void GoBack() => NavigationManager.NavigateTo(SetUrlToPreviousLocation());
        private void NavigateToEdit() => NavigationManager.NavigateTo($"/todo/edit/{Id}/Details");
        private void NavigateToEdit(Guid subTaskId) => NavigationManager.NavigateTo($"/todo/edit/{subTaskId}/Details/{Id}");

        private Color GetMudChipColor(Priority level) => level switch
        {
            Priority.VeryLow => Color.Default,
            Priority.Low => Color.Info,
            Priority.Medium => Color.Primary,
            Priority.High => Color.Warning,
            Priority.Critical => Color.Error,
            _ => Color.Default
        };

        private string SetUrlToPreviousLocation()
        {
            if (!string.IsNullOrEmpty(PreviousLocation))
            {
                return $"/{PreviousLocation.ToLower()}";
            }

            return "/todo";
        }
    }
}
