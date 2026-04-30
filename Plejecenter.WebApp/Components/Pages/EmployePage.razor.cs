using Microsoft.AspNetCore.Components;
using Plejecenter.Domain;
using Plejecenter.Shared.Enums;
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
        #endregion

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");

            AvailableRoles = (UserRole[])Enum.GetValues(typeof(UserRole));
            await LoadEmployeesAsync();
        }
        #endregion

        #region Data (API)
        private async Task LoadEmployeesAsync()
        {
            var url = string.IsNullOrWhiteSpace(searchTerm)
                ? "api/employees"
                : $"api/employees?search={System.Uri.EscapeDataString(searchTerm)}";

            var items = await http.GetFromJsonAsync<List<EmployeePageDTO.EmployeeDto>>(url) ?? new();

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
