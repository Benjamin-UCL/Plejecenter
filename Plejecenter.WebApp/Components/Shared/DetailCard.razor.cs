using Microsoft.AspNetCore.Components;
using Plejecenter.Domain;
using Plejecenter.Shared.Enums;
using Plejecenter.Shared.DTOs.ResidentAdminPage;

namespace Plejecenter.WebApp.Components.Shared
{
    public partial class DetailCard
    {
        [Inject] private HttpClient http { get; set; }
        // [Parameter] public Resident? Resident { get; set; }
        [Parameter] public ResidentAdminPageDTO.ResidentDto Resident { get; set; }
        // [Parameter] public EventCallback<Resident> ResidentChanged { get; set; }
        // [Parameter] public EventCallback<Resident> OnSaveRequested { get; set; }
        [Parameter] public EventCallback<ResidentAdminPageDTO.ResidentDto> ResidentChanged { get; set; }
        [Parameter] public EventCallback<ResidentAdminPageDTO.ResidentDto> OnSaveRequested { get; set; }
        // [Parameter] public EventCallback<PatientTime> OnPnAdded { get; set; }
        [Parameter] public EventCallback<ResidentAdminPageDTO.PatientTimeDto> OnPnAdded { get; set; }
        [Parameter] public EventCallback<int> OnPnDeleted { get; set; }
        // [Parameter] public EventCallback<ScheduleMedication> OnMedicationToggled { get; set; }
        [Parameter] public EventCallback<ResidentAdminPageDTO.ScheduleMedicationDto> OnMedicationToggled { get; set; }

        // For medication scheduling
        [Parameter] public EventCallback<AddMedScheduleRequest> OnMedScheduleAdded { get; set; }
        [Parameter] public EventCallback<int> OnMedScheduleDeleted { get; set; }

        // Update your EventCallback to send the whole request object
  

        // State for adding new medication times
        private bool isAddingMed = false;
        private DateTime newMedTime = DateTime.Today.AddHours(8); // Default to 8 AM
        // For the medication dropdown
        private List<ResidentAdminPageDTO.MedicationDto> availableMedications = new();
        private int selectedMedicationId;
        private string dosageInput = string.Empty;

        private async Task OnMedGivenChanged(ResidentAdminPageDTO.ScheduleMedicationDto med)
        {
            // Toggle the status
            med.IsGiven = !med.IsGiven;
            
            // Send it up to the parent to save to the database
            await OnMedicationToggled.InvokeAsync(med);
        }

        private string StatusClass
        {
            get
            {
                if (Resident is null) return "status-kritisk";
                if (Enum.TryParse<RiskIndicator>(Resident.Status, out var parsed))
                {
                    return parsed switch
                    {
                        RiskIndicator.High => "status-kritisk",
                        RiskIndicator.Middle => "status-ustabil",
                        RiskIndicator.Low => "status-stabil",
                        _ => "status-kritisk"
                    };
                }
                return "status-kritisk";
            }
        }

        private async Task OnStatusChanged(ChangeEventArgs e)
        {
            if (Resident is null) return;
            if (Enum.TryParse<RiskIndicator>(e.Value?.ToString(), out var newRisk))
            {
                // RECORDS FIX: Use 'with' to create an updated copy
                var updatedResident = Resident with 
                { 
                    RiskLevel = newRisk, 
                    Status = newRisk.ToString() 
                };
                await ResidentChanged.InvokeAsync(updatedResident);
            }
        }

        private DateTime? NextPnTime => Resident?.PatientTimes
        .OrderByDescending(p => p.DispensedAt)
        .FirstOrDefault() is ResidentAdminPageDTO.PatientTimeDto last
        ? last.DispensedAt.AddHours(4) // Or whatever logic you use for time between doses
        : null;

        private async Task AddPnEntry()
        {
            var newEntry = new ResidentAdminPageDTO.PatientTimeDto
            {
                DispensedAt = DateTime.Now,
                Note = "Givet via Administration",
                // MedicationDosage = ... (you might need to select this later)
            };

            // Tell the parent to save this to the database
            await OnPnAdded.InvokeAsync(newEntry);
        }

        //logik til at gemme ændringer i detaljekortet
        private async Task HandleSaveClick()
        {
            // We pass the current resident object back to the parent to handle the API call
            await OnSaveRequested.InvokeAsync(Resident);
        }
        // State for toggling the add medication time form
        private async Task ToggleAddMedMode()
        {
            isAddingMed = !isAddingMed;

            if (isAddingMed && !availableMedications.Any())
            {
                try
                {
                    // assuming MedicationsController has a Get endpoint at api/medication
                    var result = await http.GetFromJsonAsync<List<ResidentAdminPageDTO.MedicationDto>>("api/medications");
                    if (result != null) availableMedications = result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kunne ikke hente medicin: {ex.Message}");
                }
            }
        }

        // logik til at håndtere sletning af patienttider
        private async Task RequestDelete(int ptId)
        {
            await OnPnDeleted.InvokeAsync(ptId);
        }

        private async Task SaveNewMedTime()
        {
            if (selectedMedicationId == 0 || string.IsNullOrWhiteSpace(dosageInput))
            {
                // Add a small validation alert here if you want
                return;
            }

            var request = new AddMedScheduleRequest
            {
                Time = newMedTime,
                MedicationId = selectedMedicationId,
                Dosage = dosageInput
            };

            await OnMedScheduleAdded.InvokeAsync(request);
            
            // Reset fields for next time
            isAddingMed = false;
            dosageInput = string.Empty;
            selectedMedicationId = 0;
        }

        private async Task RequestDeleteMed(int id)
        {
            await OnMedScheduleDeleted.InvokeAsync(id);
        }

        // This method centralizes handling changes to any of the editable fields in the DetailCard
        // It creates a new copy of the Resident record with the updated value and sends it back to the parent component
        // This way, we keep the DetailCard stateless and let the parent component manage the actual data and API calls
        // The parent component can then decide when to call the API to save changes, for example, when a "Save" button is clicked, or even auto-save on every change if desired.
        // Note: This approach assumes that the Resident record is immutable (which it is, since it's a record), and that the parent component will handle the logic of updating the database and refreshing the data as needed.
        private async Task OnFieldChanged(string fieldName, string? newValue)
        {
            if (Resident is null) return;

            // 'with' creates a new copy of the Record with the updated value
            var updated = fieldName switch
            {
                "ShoppingDay" => Resident with { ShoppingDay = newValue ?? "" },
                "ShoppingNotes" => Resident with { ShoppingNotes = newValue ?? "" },
                "PaymentNotes" => Resident with { PaymentNotes = newValue ?? "" },
                "Message" => Resident with { Message = newValue ?? "" },
                _ => Resident
            };

            // This tells the Parent "Hey, the data changed, here is the new version!"
            await ResidentChanged.InvokeAsync(updated);
        }


    }
}