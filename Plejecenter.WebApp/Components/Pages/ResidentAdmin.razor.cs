using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Plejecenter.Domain;
using Plejecenter.Shared.DTOs.ResidentAdminPage;
using Plejecenter.WebApp.Components.Shared;
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


        //new fields and stuff for update above this
        private DateTime currentDate = DateTime.Now;
        private ResidentPicker.ResidentItem? selectedPickerResident;
        private Resident? currentResident;
        private List<ResidentPicker.ResidentItem> pickerResidents = new();



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


    //new LoadResidentsAsync
        private async Task LoadResidentsAsync()
        {
            await SetAuthHeader();
            var resp = await http.GetAsync("api/residents");

            if (resp.IsSuccessStatusCode)
            {
                isUnauthorized = false;
                residents = await resp.Content.ReadFromJsonAsync<List<ResidentAdminPageDTO.ResidentDto>>() ?? new();
                
                // --- ADD THIS LINE ---
                // We map the API DTOs to the specific format the ResidentPicker component wants
                pickerResidents = residents.Select(r => new ResidentPicker.ResidentItem 
                { 
                    Id = r.Id, 
                    Name = $"{r.FirstName} {r.LastName}",
                    Status = r.RiskLevel // Or whatever mapping your status uses
                }).ToList();
            }
            // ... rest of your error handling
            await InvokeAsync(StateHasChanged);
        }
        // trigger når du klikker på en dato i DatePicker'en, og den opdaterer currentDate som DetailCard'en viser
        private async Task OnDateChanged(DateTime newDate)
        {
            currentDate = newDate;
            // Potentially reload data for the new date here
        }
        // trigger når du klikker på en beboer i dropdownen, og den opdaterer currentResident som DetailCard'en viser
        private void OnResidentSelected(ResidentPicker.ResidentItem? selected)
        {
            selectedPickerResident = selected;
            
            if (selected == null)
            {
                currentResident = null;
                return;
            }

            // 1. Find the DTO from our loaded list using the ID
            var dto = residents.FirstOrDefault(r => r.Id == selected.Id);
            
            if (dto != null)
            {
                currentResident = new Resident 
                {
                    Id = dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Status = dto.Status,
                    RiskLevel = dto.RiskLevel,
                    ShoppingDay = dto.ShoppingDay,
                    ShoppingNotes = dto.ShoppingNotes,
                    PaymentNotes = dto.PaymentNotes,
                    Message = dto.Message,
                    PatientTimes = dto.PatientTimes.Select(pt => new PatientTime
                    {
                        Id = pt.Id,
                        DispensedAt = pt.DispensedAt,
                        Note = pt.Note
                    }).ToList()
                };
            }
        }   
        //håndterer ændringer fra DetailCard'en, og opdaterer currentResident som dropdown'en og DetailCard'en viser
        private void OnDetailChanged(Resident updatedResident)
        {
            currentResident = updatedResident;
            // Here you could call an API save method to persist changes automatically
        }



        // private async Task LoadResidentsAsync()
        // {
        //     //bruger helper igen isteden for at duplikere koden ovenpå
        //     await SetAuthHeader();
        //     // 3. Make the call
        //     var resp = await http.GetAsync("api/residents");

        //     if (resp.StatusCode == HttpStatusCode.Unauthorized)
        //     {
        //         isUnauthorized = true;
        //         await InvokeAsync(StateHasChanged);
        //         return;
        //     }

        //     if (resp.IsSuccessStatusCode)
        //     {
        //         isUnauthorized = false; // Reset if it was previously unauthorized
        //         residents = await resp.Content.ReadFromJsonAsync<List<ResidentAdminPageDTO.ResidentDto>>() ?? new();
        //     }
        //     else
        //     {
        //         // Handle other errors (404, 500, etc.)
        //         Console.WriteLine($"Failed to load residents: {resp.StatusCode}");
        //     }

        //     await InvokeAsync(StateHasChanged);
        // }
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

            // 1. Check right away. If no one is selected, just stop.
            if (id <= 0) 
            {
                Console.WriteLine("Ingen beboer valgt til sletning.");
                return;
            }
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

        //Den metode er for at tilføje en ny beboer, og den åbner modalen med tomme felter
        private void OpenAddModal()
        {
            editingId = 0; // Reset ID to indicate a NEW resident
            editingResident = new ResidentAdminPageDTO.UpdateResidentRequest
            {
                Status = "Aktiv" // Default values
            };
            showEditModal = true;
        }

        private async Task SaveEditAsync()
        {
            await SetAuthHeader();
            HttpResponseMessage resp;

            if (editingId == 0) 
            {
                // CREATE NEW
                resp = await http.PostAsJsonAsync("api/residents", editingResident);
            }
            else 
            {
                // UPDATE EXISTING
                resp = await http.PutAsJsonAsync($"api/residents/{editingId}", editingResident);
            }

            if (resp.IsSuccessStatusCode)
            {
                showEditModal = false;
                await LoadResidentsAsync(); // Refresh the list and picker
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"Error saving: {error}");
            }
        }


        private async Task SaveResidentDetailsAsync(Resident updatedResident)
        {
        try 
            {
                await SetAuthHeader();

                var request = new ResidentAdminPageDTO.UpdateResidentRequest
                {
                    Id = updatedResident.Id,
                    FirstName = updatedResident.FirstName,
                    LastName = updatedResident.LastName,
                    Status = updatedResident.RiskLevel.ToString(),
                    RiskLevel = updatedResident.RiskLevel,
                    ShoppingDay = updatedResident.ShoppingDay ?? "",
                    ShoppingNotes = updatedResident.ShoppingNotes ?? "",
                    PaymentNotes = updatedResident.PaymentNotes ?? "",
                    Message = updatedResident.Message ?? ""
                };

                var resp = await http.PutAsJsonAsync($"api/residents/{updatedResident.Id}", request);
                
                if (resp.IsSuccessStatusCode)
                {
                    await LoadResidentsAsync();
                }
            }
            catch (Exception ex)
            {
                // Now the site won't freeze! It just logs the error.
                Console.WriteLine($"Critical error during save: {ex.Message}");
            }
        }

        //metode til at håndterer PN tilføjelse
        private async Task HandlePnAddedAsync(PatientTime newEntry)
        {
            if (currentResident == null) return;

            await SetAuthHeader();

            // Now this URL will match the new method we added to the controller
            var resp = await http.PostAsJsonAsync($"api/residents/{currentResident.Id}/patienttimes", newEntry);

            if (resp.IsSuccessStatusCode)
            {
                // 1. Reload the master list from the API
                await LoadResidentsAsync(); 
                
                // 2. IMPORTANT: Re-link currentResident to the updated DTO from the list
                var updatedDto = residents.FirstOrDefault(r => r.Id == currentResident.Id);
                if (updatedDto != null)
                {
                    // This triggers the DetailCard to re-render with the new data
                    OnResidentSelected(pickerResidents.FirstOrDefault(p => p.Id == updatedDto.Id));
                }
            }
            else
            {
                Console.WriteLine("Kunne ikke gemme PN tid.");
            }
        }

        //metode til at håndterer PN sletning
        private async Task HandlePnDeletedAsync(int ptId)
        {
            await SetAuthHeader();
            
            var resp = await http.DeleteAsync($"api/residents/patienttimes/{ptId}");

            if (resp.IsSuccessStatusCode)
            {
                // Option A: Just reload everything (safest)
                await LoadResidentsAsync();
                
                // Option B: Manually remove from local list for speed
                currentResident.PatientTimes.RemoveAll(x => x.Id == ptId);
                StateHasChanged();
            }
        }

                private List<String> emptyList = new(); // pro forma
            }
        }
