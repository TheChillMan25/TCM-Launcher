using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCM_Launcher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedModpack",
                table: "GameProfiles");

            migrationBuilder.AlterColumn<double>(
                name: "PlayTime",
                table: "GameProfiles",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<uint>(
                name: "PackReleaseNumber",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "PlayTime",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "PackReleaseNumber",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(uint),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UpdatedModpack",
                table: "GameProfiles",
                type: "INTEGER",
                nullable: true);
        }
    }
}
