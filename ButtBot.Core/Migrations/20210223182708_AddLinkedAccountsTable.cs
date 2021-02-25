using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ButtBot.Core.Migrations
{
    public partial class AddLinkedAccountsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiscordUserId",
                table: "Accounts",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LinkedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DiscordId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    PlatformId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedAccounts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "DiscordUserId",
                table: "Accounts",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");
        }
    }
}
