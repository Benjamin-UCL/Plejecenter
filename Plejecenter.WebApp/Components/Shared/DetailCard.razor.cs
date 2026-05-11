using Microsoft.AspNetCore.Components;
using Plejecenter.Domain;
using Plejecenter.Shared.Enums;

namespace Plejecenter.WebApp.Components.Shared
{
    public partial class DetailCard
    {
        [Parameter] public Resident? Resident { get; set; }
        [Parameter] public EventCallback<Resident> ResidentChanged { get; set; }
        [Parameter] public EventCallback<Resident> OnSaveRequested { get; set; }
        [Parameter] public EventCallback<PatientTime> OnPnAdded { get; set; }

        private void OnMedGivenChanged(ScheduleMedication med)
        {
            // TODO: call API to register medication as given
            // e.g. await ApiService.RegisterMedGivenAsync(med.Id, DateTime.Now);
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
                Resident.RiskLevel = newRisk;
                // If you want to keep the string 'Status' in sync for the API:
                Resident.Status = newRisk.ToString();
                await ResidentChanged.InvokeAsync(Resident);
            }
        }

        private DateTime? NextPnTime => Resident?.PatientTimes
        .OrderByDescending(p => p.DispensedAt)
        .FirstOrDefault() is PatientTime last
        ? last.DispensedAt + last.TimeBetweenDosis.ToTimeSpan()
        : null;

        private async Task AddPnEntry()
        {
            var newEntry = new PatientTime
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

        // // Optional: If you want the status dropdown to save INSTANTLY without the button
        // private async Task OnStatusChanged(ChangeEventArgs e)
        // {
        //     if (Enum.TryParse<RiskIndicator>(e.Value?.ToString(), out var newStatus))
        //     {
        //         Resident.Status = newStatus;
        //         // Trigger the same save logic immediately
        //         await OnSaveRequested.InvokeAsync(Resident);
        //     }
        // }


    }
}