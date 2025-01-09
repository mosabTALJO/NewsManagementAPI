using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameNewsMaterialPublishingStrategies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsMaterialPublishStrat_NewsMaterials_NewsMaterialID",
                table: "NewsMaterialPublishStrat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsMaterialPublishStrat",
                table: "NewsMaterialPublishStrat");

            migrationBuilder.RenameTable(
                name: "NewsMaterialPublishStrat",
                newName: "NewsMaterialPublishingStrategies");

            migrationBuilder.RenameIndex(
                name: "IX_NewsMaterialPublishStrat_NewsMaterialID",
                table: "NewsMaterialPublishingStrategies",
                newName: "IX_NewsMaterialPublishingStrategies_NewsMaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsMaterialPublishingStrategies",
                table: "NewsMaterialPublishingStrategies",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsMaterialPublishingStrategies_NewsMaterials_NewsMaterial~",
                table: "NewsMaterialPublishingStrategies",
                column: "NewsMaterialID",
                principalTable: "NewsMaterials",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsMaterialPublishingStrategies_NewsMaterials_NewsMaterial~",
                table: "NewsMaterialPublishingStrategies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsMaterialPublishingStrategies",
                table: "NewsMaterialPublishingStrategies");

            migrationBuilder.RenameTable(
                name: "NewsMaterialPublishingStrategies",
                newName: "NewsMaterialPublishStrat");

            migrationBuilder.RenameIndex(
                name: "IX_NewsMaterialPublishingStrategies_NewsMaterialID",
                table: "NewsMaterialPublishStrat",
                newName: "IX_NewsMaterialPublishStrat_NewsMaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsMaterialPublishStrat",
                table: "NewsMaterialPublishStrat",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsMaterialPublishStrat_NewsMaterials_NewsMaterialID",
                table: "NewsMaterialPublishStrat",
                column: "NewsMaterialID",
                principalTable: "NewsMaterials",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
