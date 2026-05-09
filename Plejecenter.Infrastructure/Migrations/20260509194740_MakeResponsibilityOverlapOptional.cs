using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plejecenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeResponsibilityOverlapOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responsibilities_Overlaps_OverlapId",
                table: "Responsibilities");

            migrationBuilder.AlterColumn<int>(
                name: "OverlapId",
                table: "Responsibilities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Responsibilities_Overlaps_OverlapId",
                table: "Responsibilities",
                column: "OverlapId",
                principalTable: "Overlaps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responsibilities_Overlaps_OverlapId",
                table: "Responsibilities");

            migrationBuilder.AlterColumn<int>(
                name: "OverlapId",
                table: "Responsibilities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Responsibilities_Overlaps_OverlapId",
                table: "Responsibilities",
                column: "OverlapId",
                principalTable: "Overlaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
