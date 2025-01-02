using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSocialMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "SocialMedias",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "SocialMedias");
        }
    }
}
