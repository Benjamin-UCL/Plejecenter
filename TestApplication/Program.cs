using ModelsLibrary;

Console.WriteLine(":::TEST-APPLICATION STARTED:::");
Console.WriteLine();

User user1 = new User(1, "Thomas", "123", "TH");
User user2 = new User(2, "Anja", "456", "AJ");
List<User> users = new List<User>();
users.Add(user1);
users.Add(user2);

Department department = new Department(1, "Skoven");

Overlap firstshift = new Overlap(department, users);

Console.WriteLine(firstshift.department.Title);
Console.WriteLine(firstshift.Personel.Count);
foreach (User user in firstshift.Personel)
{
    Console.WriteLine(user.Alias);
}

Console.ReadKey();