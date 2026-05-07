using Microsoft.AspNetCore.Components;

namespace Plejecenter.WebApp.Components.Shared
{
    public partial class ResidentCard
    {
        [Parameter] public int ResidentId { get; set; }
        [Parameter] public string FullName { get; set; }
        [Parameter] public string Initials { get; set; }
        [Parameter] public List<string> MedicationTimes { get; set; } = new();

        [Parameter] public EventCallback<int> OnEdit { get; set; }
        [Parameter] public EventCallback<int> OnDelete { get; set; }
    }
}