using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class Versions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForgeVersions",
                columns: table => new
                {
                    VersionName = table.Column<string>(type: "TEXT", nullable: false),
                    MCVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Recommended = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgeVersions", x => x.VersionName);
                });

            migrationBuilder.CreateTable(
                name: "VanillaVersions",
                columns: table => new
                {
                    VersionName = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanillaVersions", x => x.VersionName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForgeVersions");

            migrationBuilder.DropTable(
                name: "VanillaVersions");
        }
    }
}
