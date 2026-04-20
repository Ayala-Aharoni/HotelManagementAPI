using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTabletStatusToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TabletIpAddress",
                table: "Room");

            migrationBuilder.AddColumn<bool>(
                name: "IsTabletActive",
                table: "Room",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTabletActive",
                table: "Room");

            migrationBuilder.AddColumn<string>(
                name: "TabletIpAddress",
                table: "Room",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
