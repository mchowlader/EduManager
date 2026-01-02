using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSystem.ApplicationUsers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCompositIndexSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassesId",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassesId_Name",
                table: "Sections",
                columns: new[] { "ClassesId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassesId_Name",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassesId",
                table: "Sections",
                column: "ClassesId");
        }
    }
}
