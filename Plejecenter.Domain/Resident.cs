using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Domain;

public class Resident
{
    public int Id { get; set; }
    public int SocialSecurityNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    
    public string Status { get; set; }
    public string ShoppingDay { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ShoppingNotes { get; set; } = string.Empty;
    public string PaymentNotes { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public RiskIndicator RiskLevel { get; set; }
    
    public List<ScheduleMedication> ScheduleMedications { get; set; } = new();
    public List<PatientTime> PatientTimes { get; set; } = new();

    public Department? Department { get; set; }
    public int Apartment {  get; set; }
}
    