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
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Alias { get; set; }
    
    public string Status { get; set; }
    public string ShoppingDay { get; set; }
    public string PaymentMethod { get; set; }
    public string ShoppingNotes { get; set; }
    public string PaymentNotes { get; set; }
    public string Message { get; set; }
    public RiskIndicator RiskLevel { get; set; }
    
    public List<ScheduleMedication> ScheduleMedications { get; set; } = new();
    public List<PatientTime> PatientTimes { get; set; } = new();

    public Department? Department { get; set; }
    public int Apartment {  get; set; }
}
    