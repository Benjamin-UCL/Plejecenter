using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary;

public class Overlap
{
    public int Id { get; set; }

    public DateTime date { get; set; }

    public ShiftType ShiftType { get; set; }

    public Department department { get; set; }

    public List<User> Personel {  get; set; }

    // Constructors and overloads
    public Overlap() {}
}
