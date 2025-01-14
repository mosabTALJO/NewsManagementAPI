using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class NewsSocialMediaSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Schedule",
                table: "NewsSocialMedias",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "NewsSocialMedias");
        }
    }
}
