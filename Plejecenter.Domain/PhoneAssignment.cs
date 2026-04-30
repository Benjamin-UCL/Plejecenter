using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;

public class PhoneAssignment
{
    public int Id { get; set; }
    public int PhoneNumber { get; set; }
    public Overlap Overlap { get; set; }
    public User User { get; set; }
}