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
    public DbSet<Medication> Medications { get; set; }
    public DbSet<MedicationDosage> MedicationsDosages { get; set; }
    public DbSet<PatientTime> PatientTimes { get; set; }
    public DbSet<PhoneAssignment> PhoneAssignments { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Resident> Residents { get; set; }
    public DbSet<Responsibility> Responsibilities { get; set; }
    public DbSet<ScheduleMedication> ScheduleMedications { get; set; }
    public DbSet<ShiftTask> ShiftTasks { get; set; }



}
