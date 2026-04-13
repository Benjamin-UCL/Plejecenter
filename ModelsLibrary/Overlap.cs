using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary;

public class Overlap
{
    public int Id { get; private set; }

    public DateTime date { get; private set; }

    public ShiftType ShiftType { get; private set; }

    public Department department { get; private set; }

    public List<User> Personel {  get; private set; }

    // Constructors and overloads
    public Overlap(Department department) : this(department, new List<User>())
    {
    }

    public Overlap(Department department, List<User> personel)
    {
        this.date = DateTime.Today;
        this.department = department;
        this.Personel = personel;
    }

    public void addPersonel(User user)
    {
        this.Personel.Add(user);
    }

    public void removePersonel(User user) 
    {
        this.Personel.Remove(user);
    }


}
