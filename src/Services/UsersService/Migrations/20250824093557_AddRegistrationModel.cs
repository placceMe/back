using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersService.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users_service");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "users_service");

            migrationBuilder.RenameTable(
                name: "SalerInfos",
                newName: "SalerInfos",
                newSchema: "users_service");

            migrationBuilder.CreateTable(
                name: "RegistrationUsers",
                schema: "users_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivationCode = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivationCodeExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationUsers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationUsers",
                schema: "users_service");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "users_service",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "SalerInfos",
                schema: "users_service",
                newName: "SalerInfos");
        }
    }
}
