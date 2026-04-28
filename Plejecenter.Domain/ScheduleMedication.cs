using Plejecenter.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;
public class ScheduleMedication
{
    public int Id { get; set; }
    public DateTime DispenseAt { get; set; }
    public bool IsGiven { get; set; }
    public MedicationDosage MedicationDosage { get; set; }
    public List<Day> Days { get; set; } = new List<Day>();
}