using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAAIC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IdItsonUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdInstitucional",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "IdInstitucional",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdInstitucional",
                table: "Users",
                column: "IdInstitucional",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdInstitucional",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "IdInstitucional",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdInstitucional",
                table: "Users",
                column: "IdInstitucional");
        }
    }
}
