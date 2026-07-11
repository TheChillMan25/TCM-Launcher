using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameProfileSetInstalledToNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Installed",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Installed",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");
        }
    }
}
