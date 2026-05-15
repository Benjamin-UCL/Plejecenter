using System;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Domain;

public class ResponsibilityTemplate
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ResponsibilityRecurrence Recurrence { get; set; } = ResponsibilityRecurrence.DailySingleShift;
    public ShiftMask ApplicableShifts { get; set; } = ShiftMask.Morgen;

    public List<Responsibility> Instances { get; set; } = new();
}
