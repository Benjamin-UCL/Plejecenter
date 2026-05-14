using Plejecenter.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Shared.DTOs.DisplayPage;

public class ScheduleMedicationDisplayDTO
{
    public int Id { get; set; }
    public DateTime DispenseAt { get; set; }
    public bool IsGiven { get; set; }
    public string MedicationName { get; set; }
    public string Dosage { get; set; }
    public List<Day> Days { get; set; }
}