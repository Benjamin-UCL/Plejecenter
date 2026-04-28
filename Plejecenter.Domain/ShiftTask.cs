using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;

public class ShiftTask
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool Done { get; set; }
    public Overlap Overlap { get; set; }
}
    