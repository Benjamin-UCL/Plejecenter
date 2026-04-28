using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Plejecenter.WebApp.Components.Shared;

public partial class CalendarPopover : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    [Parameter] public DateTime Value { get; set; }
    [Parameter] public EventCallback<DateTime> ValueChanged { get; set; }

    /// <summary>
    /// Kultur til månedsnavn og ugedage. Default er da-DK for "april 2026" osv.
    /// </summary>
    [Parameter] public string Culture { get; set; } = "da-DK";

    private DateTime viewMonth; // altid 1. i måneden

    private CultureInfo CultureInfo => CultureInfo.GetCultureInfo(Culture);

    private string monthLabel => viewMonth.ToString("MMMM yyyy", CultureInfo);

    private string[] WeekdayLabels => new[] { "ma", "ti", "on", "to", "fr", "lø", "sø" };

    protected override void OnParametersSet() 
    {
        // Når popover åbner eller value ændres, synkroniser viewMonth til den valgte dato.
        if (viewMonth == default || !IsSameMonth(viewMonth, Value))
            viewMonth = new DateTime(Value.Year, Value.Month, 1);
    }
    private static bool IsSameMonth(DateTime a, DateTime b) => a.Year == b.Year && a.Month == b.Month; 

    private void PrevMonth() => viewMonth = viewMonth.AddMonths(-1);
    private void NextMonth() => viewMonth = viewMonth.AddMonths(1);

    private async Task Close() => await IsOpenChanged.InvokeAsync(false);

    private async Task Select(DateTime date)
    {
        await ValueChanged.InvokeAsync(date.Date);
        await Close();
    }

    private async Task GoToToday()
    {
        await ValueChanged.InvokeAsync(DateTime.Today);
        viewMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await Close();
    }

    private sealed record Cell(DateTime? Date, bool IsEmpty, bool IsToday, bool IsSelected);

    private IReadOnlyList<Cell> Cells
    {
        get
        {
            var firstOfMonth = viewMonth;
            var daysInMonth = DateTime.DaysInMonth(firstOfMonth.Year, firstOfMonth.Month);

            // Mandag som start på ugen: Monday=0, Tuesday=1, ... Sunday=6
            var firstDayIndex = ((int)firstOfMonth.DayOfWeek + 6) % 7;  //

            var cells = new List<Cell>(42); //
            var today = DateTime.Today;
            var selected = Value.Date;

            for (var i = 0; i < 42; i++)
            {
                var dayNumber = i - firstDayIndex + 1;
                if (dayNumber < 1 || dayNumber > daysInMonth)
                {
                    cells.Add(new Cell(null, IsEmpty: true, IsToday: false, IsSelected: false));
                    continue;
                }

                var date = new DateTime(firstOfMonth.Year, firstOfMonth.Month, dayNumber);
                cells.Add(new Cell(
                    date,
                    IsEmpty: false,
                    IsToday: date.Date == today,
                    IsSelected: date.Date == selected
                ));
            }

            return cells;
        }
    }
}

