using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class EditGameProfilesAddUpdateModpackAndPlayTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "GameProfiles");

            migrationBuilder.RenameColumn(
                name: "Pinned",
                table: "GameProfiles",
                newName: "UpdatedModpack");

            migrationBuilder.RenameColumn(
                name: "PackVersionNumber",
                table: "GameProfiles",
                newName: "PlayTime");

            migrationBuilder.AddColumn<uint>(
                name: "PackReleaseNumber",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackReleaseNumber",
                table: "GameProfiles");

            migrationBuilder.RenameColumn(
                name: "UpdatedModpack",
                table: "GameProfiles",
                newName: "Pinned");

            migrationBuilder.RenameColumn(
                name: "PlayTime",
                table: "GameProfiles",
                newName: "PackVersionNumber");

            migrationBuilder.AddColumn<bool>(
                name: "LastPlayed",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: true);
        }
    }
}
