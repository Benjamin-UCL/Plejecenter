using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary;

public class Department
{
    public int Id { get; private set; }

    public string Title { get; private set; }

    public Department(int id, string Title) 
    { 
        this.Id = id;
        this.Title = Title;        
    }
}
