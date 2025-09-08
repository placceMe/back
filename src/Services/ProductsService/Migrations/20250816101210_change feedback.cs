using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsService.Migrations
{
    /// <inheritdoc />
    public partial class Changefeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "products_service");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "products_service");

            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "Feedbacks",
                newSchema: "products_service");

            migrationBuilder.RenameTable(
                name: "Characteristics",
                newName: "Characteristics",
                newSchema: "products_service");

            migrationBuilder.RenameTable(
                name: "CharacteristicDicts",
                newName: "CharacteristicDicts",
                newSchema: "products_service");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Categories",
                newSchema: "products_service");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachments",
                newSchema: "products_service");

            migrationBuilder.RenameColumn(
                name: "Rating",
                schema: "products_service",
                table: "Feedbacks",
                newName: "RatingSpeed");

            migrationBuilder.AddColumn<long>(
                name: "RatingAvailable",
                schema: "products_service",
                table: "Feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RatingAverage",
                schema: "products_service",
                table: "Feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RatingDescription",
                schema: "products_service",
                table: "Feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RatingService",
                schema: "products_service",
                table: "Feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingAvailable",
                schema: "products_service",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                schema: "products_service",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "RatingDescription",
                schema: "products_service",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "RatingService",
                schema: "products_service",
                table: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "products_service",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Feedbacks",
                schema: "products_service",
                newName: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Characteristics",
                schema: "products_service",
                newName: "Characteristics");

            migrationBuilder.RenameTable(
                name: "CharacteristicDicts",
                schema: "products_service",
                newName: "CharacteristicDicts");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "products_service",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "Attachments",
                schema: "products_service",
                newName: "Attachments");

            migrationBuilder.RenameColumn(
                name: "RatingSpeed",
                table: "Feedbacks",
                newName: "Rating");
        }
    }
}
