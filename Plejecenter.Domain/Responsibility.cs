using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plejecenter.Domain;
public class Responsibility
{
    public int Id { get; set; }
    public string Title { get; set; }
    public Overlap Overlap { get; set; }
    public User User { get; set; }
}