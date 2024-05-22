using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PasswordManager.TelegramClient.Migrations
{
    /// <inheritdoc />
    public partial class Addrequestforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TelegramUserData",
                table: "TelegramUserData");

            migrationBuilder.RenameTable(
                name: "TelegramUserData",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "InternalId");

            migrationBuilder.AlterColumn<long>(
                name: "TelegramUserId",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "FormId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "TelegramUserId");

            migrationBuilder.CreateTable(
                name: "RequestForms",
                columns: table => new
                {
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormType = table.Column<string>(type: "text", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestForms", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_RequestForms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "TelegramUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_UserId",
                table: "RequestForms",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestForms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "TelegramUserData");

            migrationBuilder.RenameColumn(
                name: "InternalId",
                table: "TelegramUserData",
                newName: "Id");

            migrationBuilder.AlterColumn<long>(
                name: "TelegramUserId",
                table: "TelegramUserData",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelegramUserData",
                table: "TelegramUserData",
                column: "Id");
        }
    }
}
