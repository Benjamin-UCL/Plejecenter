using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Plejecenter.Shared.DTOs.ResidentAdminPage;
using System.Net;
using System.Net.Http.Json;
using System.Reflection.Metadata;

namespace Plejecenter.WebApp.Components.Pages
{
    public partial class ResidentAdmin : ComponentBase
    {
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private HttpClient http = default!;
        private List<ResidentAdminPageDTO.ResidentDto> residents = new();
        private bool isUnauthorized;
        private bool hasLoadedOnce;
        private ResidentAdminPageDTO.CreateResidentRequest newResident = new();
        //brugt i HandleEditResidentAsycn
        private bool showEditModal = false;
        private ResidentAdminPageDTO.UpdateResidentRequest editingResident = new();
        private int editingId;

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || hasLoadedOnce) return;
            hasLoadedOnce = true;

            // var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");
            // if (!string.IsNullOrWhiteSpace(token))
            // {
            //     http.DefaultRequestHeaders.Authorization =
            //         new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // }

            //bruger helper
            await SetAuthHeader();

            await LoadResidentsAsync();
        }

        private async Task LoadResidentsAsync()
        {
            //bruger helper igen isteden for at duplikere koden ovenpå
            await SetAuthHeader();
            // 3. Make the call
            var resp = await http.GetAsync("api/residents");

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                isUnauthorized = true;
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (resp.IsSuccessStatusCode)
            {
                isUnauthorized = false; // Reset if it was previously unauthorized
                residents = await resp.Content.ReadFromJsonAsync<List<ResidentAdminPageDTO.ResidentDto>>() ?? new();
            }
            else
            {
                // Handle other errors (404, 500, etc.)
                Console.WriteLine($"Failed to load residents: {resp.StatusCode}");
            }

            await InvokeAsync(StateHasChanged);
        }
        #endregion

        private async Task HandleAddResidentAsync()
        { 
            //igen bruger helperen for at undgå duplikering
            await SetAuthHeader();
            // 2. EXPLICIT REQUEST DTO
            var req = new ResidentAdminPageDTO.CreateResidentRequest
            {
                FirstName = newResident.FirstName,
                LastName = newResident.LastName,
                Alias = newResident.Alias,
                Status = "Aktiv",
                Apartment = 0, // Or bind to a UI input
                SocialSecurityNumber = 0 // Or bind to a UI input
            };

            var resp = await http.PostAsJsonAsync("api/residents", req);
            
            if (resp.IsSuccessStatusCode)
            {
                newResident = new(); 
                await LoadResidentsAsync();
            }
        }

        private async Task HandleDeleteResidentAsync(int id)
        {
            // Simple browser popup check
            bool confirmed = await JS.InvokeAsync<bool>("confirm", "Er du sikker på, at du vil slette denne beboer?");
            
            if (confirmed)
            {
                await SetAuthHeader();
                var resp = await http.DeleteAsync($"api/residents/{id}");

                if (resp.IsSuccessStatusCode)
                {
                    await LoadResidentsAsync(); // Refresh list
                }
            }
        }

        private async Task SetAuthHeader()
        {
            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task HandleEditResidentAsync(int id)
        {
            editingId = id;
            var resident = residents.FirstOrDefault(r => r.Id == id);
            
            if (resident != null)
            {
                //populate the DTO with existing data
                editingResident = new ResidentAdminPageDTO.UpdateResidentRequest
                {
                    Id = resident.Id,
                    FirstName = resident.FirstName,
                    LastName = resident.LastName,
                    Alias = resident.Alias,
                    Apartment = resident.Apartment,
                    Status = resident.Status,
                    ShoppingDay = "",
                    PaymentMethod = "",
                    ShoppingNotes = "",
                    PaymentNotes = "",
                    Message = "",
                    RiskLevel = resident.RiskLevel
                };
            
            showEditModal = true;
            }

        }

        private async Task SaveEditAsync()
        {
            Console.WriteLine("Gemmer...");
            // kalder PUT endpointet
            await SetAuthHeader();

            var resp = await http.PutAsJsonAsync($"api/residents/{editingId}", editingResident);

            if (resp.IsSuccessStatusCode)
            {
                showEditModal = false;
                await LoadResidentsAsync(); // Refresh list
            }
            else
            {
                // var errorMsg = await resp.Content.ReadAsStringAsync();
                // Console.WriteLine($"Kunne ikke opdatere: {errorMsg}");
                var errorDetail = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"Kritiske detaljer fra API: {errorDetail}");
            }
        }

        private List<String> emptyList = new(); // pro forma
    }
}
