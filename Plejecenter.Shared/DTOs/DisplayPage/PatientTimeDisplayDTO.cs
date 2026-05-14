using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Shared.DTOs.DisplayPage;

public class PatientTimeDisplayDTO
{
    public int Id { get; set; }
    public DateTime DispensedAt { get; set; }
    public TimeOnly TimeBetweenDosis { get; set; }
    public string Note { get; set; }
    public string MedicationName { get; set; }
    public string Dosage { get; set; }
}
