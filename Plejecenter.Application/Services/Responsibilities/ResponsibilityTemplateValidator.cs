using Plejecenter.Shared;
using Plejecenter.Shared.DTOs.ResponsibilityPage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Application.Services.Responsibilities;

public static class ResponsibilityTemplateValidator
{
    public static string? ValidateCreate(ResponsibilityDTO.CreateTemplateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return "Titel er påkrævet.";

        var shifts = req.Recurrence == ResponsibilityRecurrence.DailyAllShifts
            ? ShiftMask.All
            : req.ApplicableShifts;

        if (shifts == ShiftMask.None)
            return "Vælg mindst ét vagtskift.";

        return req.Recurrence switch
        {
            ResponsibilityRecurrence.DailySingleShift when ResponsibilitySchedule.PopCount(shifts) != 1 =>
                "Vælg præcis ét vagtskift for 'hvert vagtskift, hver dag'.",
            ResponsibilityRecurrence.SingleDate when ResponsibilitySchedule.PopCount(shifts) < 1 =>
                "Vælg mindst ét vagtskift for den enkelte dag.",
            _ => null
        };
    }
}
