using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_AspNetUsers_NewsUserID",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_NewsUserID",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "NewsUserID",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewsUserID",
                table: "AspNetRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AspNetRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_NewsUserID",
                table: "AspNetRoles",
                column: "NewsUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_AspNetUsers_NewsUserID",
                table: "AspNetRoles",
                column: "NewsUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
