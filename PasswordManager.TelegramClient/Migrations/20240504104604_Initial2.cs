using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.TelegramClient.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasterPasswordHash",
                table: "TelegramUserData",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterPasswordHash",
                table: "TelegramUserData");
        }
    }
}
