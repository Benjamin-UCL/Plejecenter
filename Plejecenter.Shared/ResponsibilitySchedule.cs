using Plejecenter.Shared.Enums;

namespace Plejecenter.Shared;

public static class ResponsibilitySchedule
{
    public static ShiftMask ToMask(ShiftType shift) => shift switch
    {
        ShiftType.Morgen => ShiftMask.Morgen,
        ShiftType.Eftermiddag => ShiftMask.Eftermiddag,
        ShiftType.Nat => ShiftMask.Nat,
        _ => ShiftMask.None
    };

    public static ShiftType? MaskToSingleShift(ShiftMask mask)
    {
        var count = 0;
        ShiftType? single = null;
        foreach (ShiftType s in Enum.GetValues<ShiftType>())
        {
            if ((mask & ToMask(s)) != 0)
            {
                count++;
                single = s;
            }
        }

        return count == 1 ? single : null;
    }

    public static IEnumerable<ShiftType> MaskToShifts(ShiftMask mask)
    {
        foreach (ShiftType s in Enum.GetValues<ShiftType>())
        {
            if ((mask & ToMask(s)) != 0)
                yield return s;
        }
    }

    public static int PopCount(ShiftMask mask)
    {
        var n = (int)mask;
        var count = 0;
        while (n != 0)
        {
            count += n & 1;
            n >>= 1;
        }
        return count;
    }

    public static bool TemplateAppliesTo(
        DateTime day,
        ShiftType shift,
        ResponsibilityRecurrence recurrence,
        ShiftMask applicableShifts,
        DateTime startDate)
    {
        var d = day.Date;
        var start = startDate.Date;
        var bit = ToMask(shift);

        return recurrence switch
        {
            ResponsibilityRecurrence.SingleDate =>
                d == start && (applicableShifts & bit) != 0,
            ResponsibilityRecurrence.DailyAllShifts =>
                d >= start,
            ResponsibilityRecurrence.DailySingleShift =>
                d >= start && (applicableShifts & bit) != 0,
            _ => false
        };
    }

    public static ShiftMask ShiftsForCreate(
        ResponsibilityRecurrence recurrence,
        ShiftMask applicableShifts)
    {
        return recurrence switch
        {
            ResponsibilityRecurrence.DailyAllShifts => ShiftMask.All,
            _ => applicableShifts
        };
    }
}
