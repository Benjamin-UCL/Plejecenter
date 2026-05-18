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
        // private Resident? currentResident;
        private ResidentAdminPageDTO.ResidentDto? selectedResidentDto;
        private List<ResidentPicker.ResidentItem> pickerResidents = new();

        //for confirmdelete implementation
        private bool showDeleteConfirm;



        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            http = HttpClientFactory.CreateClient("SlottetApi");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || hasLoadedOnce) return;
            hasLoadedOnce = true;

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


        private void OnResidentSelected(ResidentPicker.ResidentItem? selected)
        {
            // Update the selected item from the picker
            selectedPickerResident = selected;

            if (selected == null)
            {
                selectedResidentDto = null;
                StateHasChanged();
                return;
            }
            // 1. Look into the 'residents' list (which contains the full DTOs from the API)
            // 2. Find the one that matches the ID of the person clicked in the sidebar
            var fullDto = residents.FirstOrDefault(r => r.Id == selected?.Id);

            if (fullDto != null)
            {
                // 3. Assign that DTO to our state variable
                selectedResidentDto = fullDto;
            }
            
            StateHasChanged();
        }



        //håndterer ændringer fra DetailCard'en, og opdaterer selectedResidentDto som dropdown'en og DetailCard'en viser
        private void OnDetailChanged(ResidentAdminPageDTO.ResidentDto updatedResident)
        {
            selectedResidentDto = updatedResident;
            // Here you could call an API save method to persist changes automatically
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

        // private async Task HandleDeleteResidentAsync(int id)
        // {

        //     // 1. Check right away. If no one is selected, just stop.
        //     if (id <= 0) 
        //     {
        //         Console.WriteLine("Ingen beboer valgt til sletning.");
        //         return;
        //     }
        //     // Simple browser popup check
        //     bool confirmed = await JS.InvokeAsync<bool>("confirm", "Er du sikker på, at du vil slette denne beboer?");
            
        //     if (confirmed)
        //     {
        //         await SetAuthHeader();
        //         var resp = await http.DeleteAsync($"api/residents/{id}");

        //         if (resp.IsSuccessStatusCode)
        //         {
        //             await LoadResidentsAsync(); // Refresh list
        //         }
        //     }
        // }

        private void HandleDeleteResidentAsync(int id)
        {
            // We NEED to keep selectedResidentDto populated so the 
            // Execute method knows WHO to delete later.
            if (id <= 0) return;
            
            showDeleteConfirm = true;
            StateHasChanged();
        }

        private async Task ExecuteDeleteResident()
        {
            Console.WriteLine("ExecuteDeleteResident triggered!");

            if (selectedResidentDto == null) 
            {
                Console.WriteLine("DELETE FAILED: selectedResidentDto was null when confirming.");
                return;
            }

            try 
            {
                await SetAuthHeader();
                var idToDelete = selectedResidentDto.Id;
                Console.WriteLine($"Sending DELETE request for ID: {idToDelete}");

                var resp = await http.DeleteAsync($"api/residents/{idToDelete}");

                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("DELETE successful on server.");
                    showDeleteConfirm = false; 
                    selectedResidentDto = null; 
                    selectedPickerResident = null;
                    await LoadResidentsAsync(); 
                }
                else 
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"DELETE failed on server. Status: {resp.StatusCode}, Error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during delete: {ex.Message}");
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

        // Denne metode håndterer gem-forespørgslen fra DetailCard'en, og den opdaterer beboerens detaljer i API'et
        private async Task SaveResidentDetailsAsync(ResidentAdminPageDTO.ResidentDto updatedResident)
        {
            await SetAuthHeader();

            Console.WriteLine($"Attempting to save ID: {updatedResident.Id} for {updatedResident.FirstName}");

            // We send the DTO directly to the API
            var resp = await http.PutAsJsonAsync($"api/residents/{updatedResident.Id}", updatedResident);

            if (resp.IsSuccessStatusCode)
            {
                await LoadResidentsAsync();

                selectedResidentDto = residents.FirstOrDefault(r => r.Id == updatedResident.Id);
                StateHasChanged();
                Console.WriteLine("Save successful.");
            }
            else
            {
                // Read the specific error message from the server to see WHY it failed
                var errorContent = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"SERVER REJECTED ID {updatedResident.Id}: {errorContent}");
            }
        }

        //metode til at håndterer PN tilføjelse
        private async Task HandlePnAddedAsync(ResidentAdminPageDTO.PatientTimeDto newEntry)
        {
            if (selectedResidentDto == null) return;
            await SetAuthHeader();
            var resp = await http.PostAsJsonAsync($"api/residents/{selectedResidentDto.Id}/patienttimes", newEntry);
            if (resp.IsSuccessStatusCode)
            {
                await LoadResidentsAsync();
                selectedResidentDto = residents.FirstOrDefault(r => r.Id == selectedResidentDto.Id);
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
                selectedResidentDto.PatientTimes.RemoveAll(x => x.Id == ptId);
                StateHasChanged();
            }
        }

        private async Task HandleMedicationToggledAsync(ResidentAdminPageDTO.ScheduleMedicationDto medDto)
        {
            if (selectedResidentDto == null) return;
                
                await SetAuthHeader();
                
                // Notice we changed 'PutAsJsonAsync' to 'PutAsync' 
                // and removed 'medDto' from the arguments because the server 
                // handles the logic based only on the ID in the URL.
                var resp = await http.PutAsync($"api/residents/medication/{medDto.Id}/toggle", null);

                if (!resp.IsSuccessStatusCode)
                {
                    // If the save fails (e.g., server down), refresh the list 
                    // to reset the checkbox to its original state in the UI.
                    await LoadResidentsAsync();
                    Console.WriteLine("Kunne ikke gemme status.");
                }
        }

        private async Task HandleMedAddedAsync(AddMedScheduleRequest request)
        {
            if (selectedResidentDto == null) return;
                await SetAuthHeader();

                var resp = await http.PostAsJsonAsync($"api/residents/{selectedResidentDto.Id}/medication", request);

                if (resp.IsSuccessStatusCode)
                {
                    await LoadResidentsAsync(); // Fetch from DB
                    
                    // Re-assign the current resident to the one found in the new list
                    var updated = residents.FirstOrDefault(r => r.Id == selectedResidentDto.Id);
                    if (updated != null)
                    {
                        // This forces the DetailCard to see the new ScheduleMedications list
                        // OnResidentSelected(pickerResidents.FirstOrDefault(p => p.Id == updated.Id));
                        
                        // We don't need to call the "Picker" logic here anymore. 
                        // Just assign the updated DTO directly to our state variable.
                        selectedResidentDto = updated;
                    }
                    StateHasChanged();
                }
                else 
                {
                    // Add this so you can see the error in the browser console
                    var error = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {error}");
                }
        }

        private async Task HandleMedDeletedAsync(int medId)
        {
            await SetAuthHeader();
            
            var resp = await http.DeleteAsync($"api/residents/medication/{medId}");

            if (resp.IsSuccessStatusCode)
            {
                await LoadResidentsAsync(); // Refresh data
                StateHasChanged();
            }
        }       


    }
}
