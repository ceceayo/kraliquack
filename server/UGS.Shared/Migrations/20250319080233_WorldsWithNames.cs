using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class WorldsWithNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Worlds",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Worlds");
        }
    }
}
