using Microsoft.EntityFrameworkCore;
using ModelsLibrary;
using System.Collections.Generic;

namespace API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Overlap> Overlaps { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
}
