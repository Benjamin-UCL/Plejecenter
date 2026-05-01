using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Plejecenter.Domain;
using Plejecenter.Shared.Enums;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Plejecenter.Shared.DTOs.EmployePage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plejecenter.WebApp.Components.Pages
{
    public partial class EmployePage : ComponentBase
    {
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private HttpClient http = default!; // Initialized in OnInitializedAsync for better control over BaseAddress

        #region Fields / Properties
        public List<User> Employees { get; private set; } = new List<User>();
        public User newEmployee = new User();
        private bool isEditing;
        private bool isAddEmployeeFormVisible = false;
        private string searchTerm = string.Empty;
        private UserRole[] AvailableRoles;
        private bool showDeleteConfirmation = false;
        private User employeeToDelete;
        private bool isUnauthorized;
        private bool hasLoadedOnce;
        #endregion

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");

            AvailableRoles = (UserRole[])Enum.GetValues(typeof(UserRole));
            // Vigtigt: vi loader IKKE data her, fordi Blazor Server kan være midt i rendering,
            // og AuthHeaderHandler bruger JS interop til at hente JWT fra localStorage.
            // Hvis det sker for tidligt kan requesten blive sendt uden token -> 401 -> circuit crasher.
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || hasLoadedOnce) return;
            hasLoadedOnce = true;

            // Hent token fra browserens localStorage EFTER første render.
            // (JS interop er ikke stabilt/tilgængeligt tidligere i lifecycle i Blazor Server.)
            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            await LoadEmployeesAsync();
        }
        #endregion

        #region Data (API)
        private async Task LoadEmployeesAsync()
        {
            var url = string.IsNullOrWhiteSpace(searchTerm)
                ? "api/employees"
                : $"api/employees?search={System.Uri.EscapeDataString(searchTerm)}";

            var resp = await http.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Hvis token mangler/er udløbet, så redirecter vi til login i stedet for at crashe.
                isUnauthorized = true;
                StateHasChanged();
                return;
            }

            resp.EnsureSuccessStatusCode();
            var items = await resp.Content.ReadFromJsonAsync<List<EmployeePageDTO.EmployeeDto>>() ?? new();

            // Vi bruger User i UI (validering + muterbar table state)
            Employees = items.Select(d => new User
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Alias = d.Alias,
                Role = d.Role,
                ActiveDeactive = d.ActiveDeactive
            }).ToList();

            // Vi loader første gang i OnAfterRenderAsync (altså EFTER en render).
            // Derfor skal vi selv trigge en ny render, ellers bliver tabellen først opdateret ved næste UI-event (klik/søg).
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region UI-handling (Form + Actions)
        private async Task HandleSubmit()
        {
            if (isEditing)
            {
                await UpdateEmployeeAsync();
            }
            else
            {
                await AddNewEmployeeAsync();
            }
        }

        private async Task AddNewEmployeeAsync()
        {
            var req = new EmployeePageDTO.CreateEmployeeRequest(
                FirstName: newEmployee.FirstName,
                LastName: newEmployee.LastName,
                Alias: newEmployee.Alias,
                Password: newEmployee.Password,
                Role: newEmployee.Role
            );

            var resp = await http.PostAsJsonAsync("api/employees", req);
            resp.EnsureSuccessStatusCode();

            ClearForm();
            await LoadEmployeesAsync();
        }

        private void EditEmployee(User user)
        {
            newEmployee = new User { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Alias = user.Alias, Password = user.Password, Role = user.Role, ActiveDeactive = user.ActiveDeactive };
            isEditing = true;
            isAddEmployeeFormVisible = true;
        }

        private async Task UpdateEmployeeAsync()
        {
            var req = new EmployeePageDTO.UpdateEmployeeRequest(
                FirstName: newEmployee.FirstName,
                LastName: newEmployee.LastName,
                Alias: newEmployee.Alias,
                Role: newEmployee.Role,
                Password: string.IsNullOrWhiteSpace(newEmployee.Password) ? null : newEmployee.Password
            );

            var resp = await http.PutAsJsonAsync($"api/employees/{newEmployee.Id}", req);
            resp.EnsureSuccessStatusCode();

            ClearForm();
            await LoadEmployeesAsync();
        }

        private void ConfirmDelete(User user)
        {
            employeeToDelete = user;
            showDeleteConfirmation = true;
        }

        private void CancelDelete()
        {
            employeeToDelete = null;
            showDeleteConfirmation = false;
        }

        private async Task ExecuteDelete()
        {
            if (employeeToDelete != null)
            {
                var resp = await http.DeleteAsync($"api/employees/{employeeToDelete.Id}");
                resp.EnsureSuccessStatusCode();
            }
            CancelDelete();
            await LoadEmployeesAsync();
        }

        private void ClearForm()
        {
            newEmployee = new User(); 
            isEditing = false;
            isAddEmployeeFormVisible = false;
        }

        private void ShowAddEmployeeForm()
        {
            newEmployee = new User();
            isEditing = false;
            isAddEmployeeFormVisible = true;
        }

        private void ToggleAddEmployeeForm()
        {
            if (isAddEmployeeFormVisible)
            {
                ClearForm();
            }
            else
            {
                ShowAddEmployeeForm();
            }
        }
        #endregion

        #region UI-handling (Inline changes)
        private async Task OnActiveChanged(User employee, bool next)
        {
            employee.ActiveDeactive = next;

            var resp = await http.PatchAsJsonAsync(
                $"api/employees/{employee.Id}/active",
                new EmployeePageDTO.SetEmployeeActiveRequest(next));

            resp.EnsureSuccessStatusCode();
        }

        private async Task OnRoleChanged(User employee, UserRole next)
        {
            employee.Role = next;

            // PUT med nuværende værdier; password sendes ikke ved inline role-change
            var resp = await http.PutAsJsonAsync(
                $"api/employees/{employee.Id}",
                new EmployeePageDTO.UpdateEmployeeRequest(
                    employee.FirstName,
                    employee.LastName,
                    employee.Alias,
                    employee.Role,
                    Password: null));

            resp.EnsureSuccessStatusCode();
        }
        #endregion

        #region Afledt data (filtrering - fallback)
        private List<User> FilteredEmployees =>
        string.IsNullOrWhiteSpace(searchTerm)
            ? Employees
            : Employees.Where(e =>
                e.FirstName.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                e.Alias.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)).ToList();
        #endregion
    }
}
