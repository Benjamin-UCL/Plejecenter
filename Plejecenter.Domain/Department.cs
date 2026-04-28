using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;

public class Department
{
    public int Id { get; set; }

    public string Title { get; set; }
    public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    public Department() 
    {       
    }
}
