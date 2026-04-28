using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;

public class Reminder
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Department> Departments { get; set; } = new List<Department>();
}
