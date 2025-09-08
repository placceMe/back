using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsService.Migrations
{
    /// <inheritdoc />
    public partial class AddParamsProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                schema: "products_service",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Producer",
                schema: "products_service",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNew",
                schema: "products_service",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Producer",
                schema: "products_service",
                table: "Products");
        }
    }
}
