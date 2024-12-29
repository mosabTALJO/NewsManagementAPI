using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //    table: "AspNetRoleClaims");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //    table: "AspNetUserRoles");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_AspNetRoles",
            //    table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "AspNetRoles");
            //migrationBuilder.DropColumn(
            //    name: "Id",
            //    table: "AspNetRoles");

            //migrationBuilder.RenameColumn(
            //    name: "ID",
            //    table: "AspNetRoles",
            //    newName: "Id");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Id",
            //    table: "AspNetRoles",
            //    type: "text",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(int),
            //    oldType: "integer",
            //    oldNullable: true);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_AspNetRoles",
            //    table: "AspNetRoles",
            //    column: "Id");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId",
            //    principalTable: "AspNetRoles",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId",
            //    principalTable: "AspNetRoles",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //    table: "AspNetRoleClaims");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //    table: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "Notifications");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_AspNetRoles",
            //    table: "AspNetRoles");

            //migrationBuilder.RenameColumn(
            //    name: "Id",
            //    table: "AspNetRoles",
            //    newName: "ID");

            //migrationBuilder.AlterColumn<int>(
            //    name: "ID",
            //    table: "AspNetRoles",
            //    type: "integer",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ID",
                table: "AspNetRoles",
                type: "text",
                nullable: false,
                defaultValue: "");
            //migrationBuilder.AddColumn<string>(
            //    name: "Id",
            //    table: "AspNetRoles",
            //    type: "text",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_AspNetRoles",
            //    table: "AspNetRoles",
            //    column: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId",
            //    principalTable: "AspNetRoles",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId",
            //    principalTable: "AspNetRoles",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
