using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Plejecenter.Shared.Enums;

namespace Plejecenter.WebApp.Components.Shared
{
    public partial class ResidentPicker
    {
        [Parameter] public List<ResidentItem> Residents { get; set; } = new();
        [Parameter] public ResidentItem? SelectedResident { get; set; }
        [Parameter] public EventCallback<ResidentItem> SelectedResidentChanged { get; set; }

        private string searchQuery = string.Empty;
        private int focusedIndex = -1;

        private IEnumerable<ResidentItem> FilteredResidents =>
            string.IsNullOrWhiteSpace(searchQuery)
                ? Residents
                : Residents.Where(r => r.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

        private async Task SelectResident(ResidentItem resident)
        {
            SelectedResident = resident;
            await SelectedResidentChanged.InvokeAsync(resident);
            focusedIndex = FilteredResidents.ToList().IndexOf(resident);
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            var list = FilteredResidents.ToList();
            if (!list.Any()) return;

            switch (e.Key)
            {
                case "ArrowDown":
                    focusedIndex = Math.Min(focusedIndex + 1, list.Count - 1);
                    break;
                case "ArrowUp":
                    focusedIndex = Math.Max(focusedIndex - 1, 0);
                    break;
                case "Enter":
                    if (focusedIndex >= 0 && focusedIndex < list.Count)
                        await SelectResident(list[focusedIndex]);
                    break;
                case "Escape":
                    searchQuery = string.Empty;
                    focusedIndex = -1;
                    break;
            }
        }

        private static string GetInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : name.Length >= 2 ? name[..2] : name;
        }

        private static string GetStatusClass(RiskIndicator status) => status switch
        {
            RiskIndicator.High => "kritisk",
            RiskIndicator.Middle => "ustabil",
            RiskIndicator.Low => "stabil",
            _ => "stabil"
        };

        private static string GetStatusLabel(RiskIndicator status) => status switch
        {
            RiskIndicator.High => "Kritisk",
            RiskIndicator.Middle => "Ustabil",
            RiskIndicator.Low => "Stabil",
            _ => "Stabil"
        };

        /// <summary>
        /// Simple model for a resident entry. 
        /// Replace with your actual domain model if it already exists.
        /// </summary>
        public class ResidentItem
        {
            public string Name { get; set; } = string.Empty;
            public RiskIndicator Status { get; set; } = RiskIndicator.Low;
        }

    }
}