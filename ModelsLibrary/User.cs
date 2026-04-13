using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary;

public class User
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Password { get; private set; }
    public string Alias { get; private set; }

    public User(int id, string name, string password, string alias)
    {
        Id = id;
        Name = name;
        Password = password;
        Alias = alias;
    }
}
