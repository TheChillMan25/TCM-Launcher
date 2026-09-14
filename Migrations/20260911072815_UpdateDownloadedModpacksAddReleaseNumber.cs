using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDownloadedModpacksAddReleaseNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "ReleaseNumber",
                table: "DownloadedModpacks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseNumber",
                table: "DownloadedModpacks");
        }
    }
}
