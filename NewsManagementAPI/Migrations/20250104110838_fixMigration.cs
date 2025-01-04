using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class fixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserType",
                table: "AspNetUsers",
                newName: "ReporterType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReporterType",
                table: "AspNetUsers",
                newName: "UserType");
        }
    }
}
