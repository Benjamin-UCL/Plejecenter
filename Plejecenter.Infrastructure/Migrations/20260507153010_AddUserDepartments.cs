using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plejecenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Residents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "UserDepartments",
                columns: table => new
                {
                    DepartmentsId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartments", x => new { x.DepartmentsId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserDepartments_Departments_DepartmentsId",
                        column: x => x.DepartmentsId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartments_UserId",
                table: "UserDepartments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents");

            migrationBuilder.DropTable(
                name: "UserDepartments");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Residents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
