using Microsoft.AspNetCore.Components;
using Plejecenter.Domain;
using Plejecenter.Shared.Enums;

namespace Plejecenter.WebApp.Components.Shared
{
    public partial class DetailCard
    {
        [Parameter] public Resident? Resident { get; set; }
        [Parameter] public EventCallback<Resident> ResidentChanged { get; set; }

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
            if (Enum.TryParse<RiskIndicator>(e.Value?.ToString(), out var parsed))
            {
                Resident.Status = parsed.ToString();
                await ResidentChanged.InvokeAsync(Resident);
            }
        }

        private DateTime? NextPnTime => Resident?.PatientTimes
        .OrderByDescending(p => p.DispensedAt)
        .FirstOrDefault() is PatientTime last
        ? last.DispensedAt + last.TimeBetweenDosis.ToTimeSpan()
        : null;

        private void AddPnEntry()
        {
            Resident?.PatientTimes.Add(new PatientTime
            {
                DispensedAt = DateTime.Now,
                Note = string.Empty
            });
        }

    }
}