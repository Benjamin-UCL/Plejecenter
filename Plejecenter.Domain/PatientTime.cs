using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;

public class PatientTime
{
    public int Id { get; set; }
    public DateTime DispensedAt { get; set; }
    public TimeOnly TimeBetweenDosis { get; set; }
    public string Note { get; set; }
    public MedicationDosage MedicationDosage;
}