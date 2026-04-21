using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary;

public class MedicationDosage
{
    public int Id { get; set; }
    public string Dosage { get; set; }
    public Medication Medication { get; set; }
}
