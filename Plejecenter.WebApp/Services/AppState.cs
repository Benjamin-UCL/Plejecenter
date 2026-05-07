using Plejecenter.Domain;

namespace Plejecenter.WebApp.Services;

public record DepartmentOption(int Id, string Title);

public class AppState
{
    public Overlap? CurrentOverlap { get; set; }

    public int? SelectedDepartmentId { get; set; }
    public string? SelectedDepartmentName { get; set; }

    // Populated when the user belongs to multiple departments; consumed by DepartmentSelect page.
    public List<DepartmentOption> PendingDepartments { get; set; } = new();
}
