using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plejecenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationDosageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicationDosageId",
                table: "PatientTimes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PatientTimes_MedicationDosageId",
                table: "PatientTimes",
                column: "MedicationDosageId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientTimes_MedicationsDosages_MedicationDosageId",
                table: "PatientTimes",
                column: "MedicationDosageId",
                principalTable: "MedicationsDosages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientTimes_MedicationsDosages_MedicationDosageId",
                table: "PatientTimes");

            migrationBuilder.DropIndex(
                name: "IX_PatientTimes_MedicationDosageId",
                table: "PatientTimes");

            migrationBuilder.DropColumn(
                name: "MedicationDosageId",
                table: "PatientTimes");
        }
    }
}
