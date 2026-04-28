using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plejecenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentReminder_Reminder_RemindersId",
                table: "DepartmentReminder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder");

            migrationBuilder.RenameTable(
                name: "Reminder",
                newName: "Reminders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrepName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<int>(type: "int", nullable: false),
                    OverlapId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneAssignments_Overlaps_OverlapId",
                        column: x => x.OverlapId,
                        principalTable: "Overlaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhoneAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Residents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocialSecurityNumber = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShoppingDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShoppingNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Apartment = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Residents_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Responsibilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverlapId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsibilities_Overlaps_OverlapId",
                        column: x => x.OverlapId,
                        principalTable: "Overlaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Responsibilities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Done = table.Column<bool>(type: "bit", nullable: false),
                    OverlapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftTasks_Overlaps_OverlapId",
                        column: x => x.OverlapId,
                        principalTable: "Overlaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationsDosages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationsDosages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationsDosages_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeBetweenDosis = table.Column<TimeOnly>(type: "time", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResidentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientTimes_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleMedications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispenseAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGiven = table.Column<bool>(type: "bit", nullable: false),
                    MedicationDosageId = table.Column<int>(type: "int", nullable: false),
                    Days = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResidentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleMedications_MedicationsDosages_MedicationDosageId",
                        column: x => x.MedicationDosageId,
                        principalTable: "MedicationsDosages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleMedications_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationsDosages_MedicationId",
                table: "MedicationsDosages",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTimes_ResidentId",
                table: "PatientTimes",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneAssignments_OverlapId",
                table: "PhoneAssignments",
                column: "OverlapId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneAssignments_UserId",
                table: "PhoneAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_DepartmentId",
                table: "Residents",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_OverlapId",
                table: "Responsibilities",
                column: "OverlapId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_UserId",
                table: "Responsibilities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMedications_MedicationDosageId",
                table: "ScheduleMedications",
                column: "MedicationDosageId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMedications_ResidentId",
                table: "ScheduleMedications",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTasks_OverlapId",
                table: "ShiftTasks",
                column: "OverlapId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentReminder_Reminders_RemindersId",
                table: "DepartmentReminder",
                column: "RemindersId",
                principalTable: "Reminders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentReminder_Reminders_RemindersId",
                table: "DepartmentReminder");

            migrationBuilder.DropTable(
                name: "PatientTimes");

            migrationBuilder.DropTable(
                name: "PhoneAssignments");

            migrationBuilder.DropTable(
                name: "Responsibilities");

            migrationBuilder.DropTable(
                name: "ScheduleMedications");

            migrationBuilder.DropTable(
                name: "ShiftTasks");

            migrationBuilder.DropTable(
                name: "MedicationsDosages");

            migrationBuilder.DropTable(
                name: "Residents");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "Reminder");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentReminder_Reminder_RemindersId",
                table: "DepartmentReminder",
                column: "RemindersId",
                principalTable: "Reminder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
