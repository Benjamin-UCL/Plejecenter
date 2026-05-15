using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plejecenter.Shared;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Application.Tests;

[TestClass]
public class ResponsibilityScheduleTests
{
    private static readonly DateTime Start = new(2026, 5, 10);

    [TestMethod]
    public void DailySingleShift_OnlyOnMatchingShift_FromStartDate()
    {
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Morgen, ResponsibilityRecurrence.DailySingleShift, ShiftMask.Morgen, Start));
        Assert.IsFalse(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Nat, ResponsibilityRecurrence.DailySingleShift, ShiftMask.Morgen, Start));
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start.AddDays(1), ShiftType.Morgen, ResponsibilityRecurrence.DailySingleShift, ShiftMask.Morgen, Start));
        Assert.IsFalse(ResponsibilitySchedule.TemplateAppliesTo(
            Start.AddDays(-1), ShiftType.Morgen, ResponsibilityRecurrence.DailySingleShift, ShiftMask.Morgen, Start));
    }

    [TestMethod]
    public void DailyAllShifts_OnEveryShift_FromStartDate()
    {
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Morgen, ResponsibilityRecurrence.DailyAllShifts, ShiftMask.All, Start));
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Nat, ResponsibilityRecurrence.DailyAllShifts, ShiftMask.All, Start));
    }

    [TestMethod]
    public void SingleDate_OnlyOnExactDay_AndSelectedShifts()
    {
        var mask = ShiftMask.Morgen | ShiftMask.Eftermiddag;
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Morgen, ResponsibilityRecurrence.SingleDate, mask, Start));
        Assert.IsTrue(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Eftermiddag, ResponsibilityRecurrence.SingleDate, mask, Start));
        Assert.IsFalse(ResponsibilitySchedule.TemplateAppliesTo(
            Start, ShiftType.Nat, ResponsibilityRecurrence.SingleDate, mask, Start));
        Assert.IsFalse(ResponsibilitySchedule.TemplateAppliesTo(
            Start.AddDays(1), ShiftType.Morgen, ResponsibilityRecurrence.SingleDate, mask, Start));
    }
}
