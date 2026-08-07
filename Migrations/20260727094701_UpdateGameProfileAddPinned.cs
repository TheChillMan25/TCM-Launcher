using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameProfileAddPinned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pinned",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pinned",
                table: "GameProfiles");
        }
    }
}
