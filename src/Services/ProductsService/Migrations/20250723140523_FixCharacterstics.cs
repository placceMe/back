using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsService.Migrations
{
    /// <inheritdoc />
    public partial class FixCharacterstics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Characteristics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Characteristics",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
