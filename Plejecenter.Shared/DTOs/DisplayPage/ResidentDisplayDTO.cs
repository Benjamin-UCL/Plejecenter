using Plejecenter.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Shared.DTOs.DisplayPage;

public class ResidentDisplayDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Alias { get; set; }
    public string Status { get; set; }
    public int Apartment { get; set; }
    public RiskIndicator RiskLevel { get; set; }
    public List<ScheduleMedicationDisplayDTO> ScheduleMedications { get; set; }
    public List<PatientTimeDisplayDTO> PatientTimes { get; set; }
}
