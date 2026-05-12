using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plejecenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibilityTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents");

            migrationBuilder.DropForeignKey(
                name: "FK_Responsibilities_Users_UserId",
                table: "Responsibilities");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Responsibilities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Responsibilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Shift",
                table: "Responsibilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Responsibilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaskDate",
                table: "Responsibilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Responsibilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Residents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ResponsibilityTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_TemplateId",
                table: "Responsibilities",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Responsibilities_ResponsibilityTemplates_TemplateId",
                table: "Responsibilities",
                column: "TemplateId",
                principalTable: "ResponsibilityTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Responsibilities_Users_UserId",
                table: "Responsibilities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_Departments_DepartmentId",
                table: "Residents");

            migrationBuilder.DropForeignKey(
                name: "FK_Responsibilities_ResponsibilityTemplates_TemplateId",
                table: "Responsibilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Responsibilities_Users_UserId",
                table: "Responsibilities");

            migrationBuilder.DropTable(
                name: "ResponsibilityTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Responsibilities_TemplateId",
                table: "Responsibilities");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Responsibilities");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "Responsibilities");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Responsibilities");

            migrationBuilder.DropColumn(
                name: "TaskDate",
                table: "Responsibilities");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Responsibilities");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Responsibilities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Responsibilities_Users_UserId",
                table: "Responsibilities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
