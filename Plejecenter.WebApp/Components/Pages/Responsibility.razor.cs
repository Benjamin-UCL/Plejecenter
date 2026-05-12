using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Plejecenter.Shared.DTOs.EmployePage;
using Plejecenter.Shared.DTOs.ResponsibilityPage;
using Plejecenter.Shared.Enums;
using Plejecenter.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Plejecenter.WebApp.Components.Pages
{
    public partial class Responsibility : ComponentBase
    {
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private HttpClient http = default!;
        private const string ResponsibilityApiBase = "api/responsibilities";

        #region Felter/State (UI)
        private List<Plejecenter.Domain.Responsibility> currentTasks = new List<Plejecenter.Domain.Responsibility>();
        private List<User> employees = new List<User>();

        private bool isAddFormVisible = false;
        private string newTaskName = "";

        private DateTime currentDate = DateTime.Now;
        private ShiftType activeShift = ShiftType.Morgen;
        private TimeSpan shiftStart;
        private TimeSpan shiftEnd;

        private bool isUnauthorized;
        private string? loadError;
        private bool hasLoadedOnce;
        #endregion

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");
            
            int currentHour = DateTime.Now.Hour;
            if (currentHour >= 7 && currentHour < 15)
                activeShift = ShiftType.Morgen;
            else if (currentHour >= 15 && currentHour < 23)
                activeShift = ShiftType.Eftermiddag;
            else
                activeShift = ShiftType.Nat;

            ApplyShiftWindow(activeShift);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || hasLoadedOnce) return;
            hasLoadedOnce = true;

            // Sæt auth-header efter første render (JS interop er stabilt her)
            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            loadError = null;

            await LoadEmployeesAsync();
            if (!isUnauthorized && string.IsNullOrEmpty(loadError))
                await LoadTasksForCurrentShiftAsync();

            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region UI-handling (Tilføj opgave)
        private void ToggleAddForm()
        {
            isAddFormVisible = !isAddFormVisible;
        }

        private void CancelAdd()
        {
            isAddFormVisible = false;
            newTaskName = "";
        }

        private async Task SaveTask()
        {
            if (string.IsNullOrWhiteSpace(newTaskName))
                return;

            var req = new ResponsibilityDTO.CreateTemplateRequest(
                Title: newTaskName.Trim(),
                StartDate: currentDate.Date,
                Shift: activeShift);

            var resp = await http.PostAsJsonAsync(ResponsibilityApiBase, req);
            resp.EnsureSuccessStatusCode();

            isAddFormVisible = false;
            newTaskName = "";
            await LoadTasksForCurrentShiftAsync();
        }
        #endregion

        #region Data (API)
        private async Task LoadEmployeesAsync()
        {
            try
            {
                var resp = await http.GetAsync("api/employees");
                if (!await HandleApiResponseAsync(resp, "Kunne ikke hente medarbejdere"))
                    return;

                var items = await resp.Content.ReadFromJsonAsync<List<EmployeePageDTO.EmployeeDto>>() ?? new();
                employees = items.Select(e => new User
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Alias = e.Alias,
                    Role = e.Role,
                    ActiveDeactive = e.ActiveDeactive
                }).ToList();
            }
            catch (Exception ex)
            {
                loadError = $"Kunne ikke hente medarbejdere ({ex.GetType().Name}). Tjek at WebApp peger på det rigtige API (fx Api:BaseUrl i appsettings).";
            }
        }

        private async Task LoadTasksForCurrentShiftAsync()
        {
            if (isUnauthorized) return;

            try
            {
                var date = Uri.EscapeDataString(currentDate.Date.ToString("yyyy-MM-dd"));
                var shift = Uri.EscapeDataString(activeShift.ToString());
                var url = $"{ResponsibilityApiBase}?date={date}&shift={shift}";

                var resp = await http.GetAsync(url);
                if (!await HandleApiResponseAsync(resp, "Kunne ikke hente ansvarsopgaver"))
                {
                    currentTasks = new List<Plejecenter.Domain.Responsibility>();
                    return;
                }

                var items = await resp.Content.ReadFromJsonAsync<List<ResponsibilityDTO.ResponsibilityDto>>() ?? new();

                currentTasks = items.Select(t => new Plejecenter.Domain.Responsibility
                {
                    Id = t.Id,
                    TemplateId = t.TemplateId,
                    Title = t.Title,
                    SortOrder = t.SortOrder,
                    TaskDate = t.TaskDate,
                    Shift = t.Shift,
                    UserId = t.UserId,
                    IsCompleted = t.IsCompleted
                })
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                loadError = $"Kunne ikke hente ansvarsopgaver ({ex.GetType().Name}). Ofte skyldes det API/database (kør migrationer mod SQL Server eller genstart API-container).";
                currentTasks = new List<Plejecenter.Domain.Responsibility>();
            }
        }

        /// <summary>
        /// Undgår HttpRequestException fra GetFromJsonAsync: 401 -> login, andre fejl -> vis besked i UI.
        /// </summary>
        private async Task<bool> HandleApiResponseAsync(HttpResponseMessage resp, string context)
        {
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                isUnauthorized = true;
                return false;
            }

            if (resp.IsSuccessStatusCode)
                return true;

            var body = await resp.Content.ReadAsStringAsync();
            loadError =
                $"{context}: API svarede {(int)resp.StatusCode} {resp.ReasonPhrase}. " +
                "Typisk årsag: manglende gyldig JWT, API kører ikke, eller Entity Framework migrationer er ikke kørt mod databasen.";
            if (!string.IsNullOrWhiteSpace(body) && body.Length < 400)
                loadError += $" ({body.Trim()})";

            return false;
        }
        #endregion

        #region Dato + vagt (navigation/valg)
        
        private async Task OnDateChanged(DateTime next)
        {
            currentDate = next;
            loadError = null;
            await LoadTasksForCurrentShiftAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task SetShift(ShiftType shift)
        {
            activeShift = shift;
            ApplyShiftWindow(shift);
            loadError = null;
            await LoadTasksForCurrentShiftAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void ApplyShiftWindow(ShiftType shift)
        {
            switch (shift)
            {
                case ShiftType.Morgen:
                    shiftStart = new TimeSpan(7, 0, 0);
                    shiftEnd = new TimeSpan(14, 59, 59);
                    break;
                case ShiftType.Eftermiddag:
                    shiftStart = new TimeSpan(15, 0, 0);
                    shiftEnd = new TimeSpan(22, 59, 59);
                    break;
                case ShiftType.Nat:
                    shiftStart = new TimeSpan(23, 0, 0);
                    shiftEnd = new TimeSpan(6, 59, 59);
                    break;
            }
        }
        #endregion

        #region Tabel handlinger (API)
        private async Task OnCompletedChanged(Plejecenter.Domain.Responsibility task, bool isCompleted)
        {
            bool previous = task.IsCompleted;
            task.IsCompleted = isCompleted;

            var resp = await http.PatchAsJsonAsync(
                $"{ResponsibilityApiBase}/{task.Id}/completed",
                new ResponsibilityDTO.SetCompletedRequest(isCompleted));

            if (!resp.IsSuccessStatusCode)
            {
                task.IsCompleted = previous;
                resp.EnsureSuccessStatusCode();
            }
        }

        private async Task OnUserChanged(Plejecenter.Domain.Responsibility task, int? userId)
        {
            int? previous = task.UserId;
            task.UserId = userId;

            var resp = await http.PutAsJsonAsync(
                $"{ResponsibilityApiBase}/{task.Id}",
                new ResponsibilityDTO.UpdateResponsibilityRequest(
                    Title: task.Title,
                    UserId: userId));

            if (!resp.IsSuccessStatusCode)
            {
                task.UserId = previous;
                resp.EnsureSuccessStatusCode();
            }
        }

        private async Task MoveTaskUp(Plejecenter.Domain.Responsibility task)
        {
            await MoveTask(task, ResponsibilityDTO.MoveDirection.Up);
        }

        private async Task MoveTaskDown(Plejecenter.Domain.Responsibility task)
        {
            await MoveTask(task, ResponsibilityDTO.MoveDirection.Down);
        }

        private async Task MoveTask(Plejecenter.Domain.Responsibility task, ResponsibilityDTO.MoveDirection direction)
        {
            var resp = await http.PostAsJsonAsync(
                $"{ResponsibilityApiBase}/{task.Id}/move",
                new ResponsibilityDTO.MoveRequest(direction));
            resp.EnsureSuccessStatusCode();
            await LoadTasksForCurrentShiftAsync();
        }

        private async Task DeleteTask(Plejecenter.Domain.Responsibility task)
        {
            var resp = await http.DeleteAsync($"{ResponsibilityApiBase}/{task.Id}");
            resp.EnsureSuccessStatusCode();
            await LoadTasksForCurrentShiftAsync();
        }
        #endregion
            }
}