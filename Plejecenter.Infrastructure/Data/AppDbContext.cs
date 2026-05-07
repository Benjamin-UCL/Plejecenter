using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;

namespace Plejecenter.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Overlap> Overlaps => Set<Overlap>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationDosage> MedicationsDosages => Set<MedicationDosage>();
    public DbSet<PatientTime> PatientTimes => Set<PatientTime>();
    public DbSet<PhoneAssignment> PhoneAssignments => Set<PhoneAssignment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Responsibility> Responsibilities => Set<Responsibility>();
    public DbSet<ScheduleMedication> ScheduleMedications => Set<ScheduleMedication>();
    public DbSet<ShiftTask> ShiftTasks => Set<ShiftTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Departments)
            .WithMany()
            .UsingEntity("UserDepartments");
    }
}

