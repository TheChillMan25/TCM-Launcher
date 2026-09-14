using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDownloadedModpacksAddModpackId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModpackId",
                table: "GameProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameProfiles_ModpackId",
                table: "GameProfiles",
                column: "ModpackId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameProfiles_DownloadedModpacks_ModpackId",
                table: "GameProfiles",
                column: "ModpackId",
                principalTable: "DownloadedModpacks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameProfiles_DownloadedModpacks_ModpackId",
                table: "GameProfiles");

            migrationBuilder.DropIndex(
                name: "IX_GameProfiles_ModpackId",
                table: "GameProfiles");

            migrationBuilder.DropColumn(
                name: "ModpackId",
                table: "GameProfiles");
        }
    }
}
