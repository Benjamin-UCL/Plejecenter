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

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || hasLoadedOnce) return;
            hasLoadedOnce = true;

            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            await LoadResidentsAsync();
        }

        private async Task LoadResidentsAsync()
        {
            // 1. Fetch the token from the browser's local storage
            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                // 2. Attach the token to the HttpClient headers
                http.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

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
        // 1. FRESH TOKEN (Just like teammate)
            var token = await JS.InvokeAsync<string?>("localStorage.getItem", "authToken");
            http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

        private List<String> emptyList = new(); // pro forma
    }
}
