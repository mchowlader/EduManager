using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSystem.ApplicationUsers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassesUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Classes_Id",
                table: "Classes",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Name",
                table: "Classes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Classes_Id",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_Name",
                table: "Classes");
        }
    }
}
