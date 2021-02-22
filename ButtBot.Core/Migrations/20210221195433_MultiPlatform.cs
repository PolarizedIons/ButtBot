using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ButtBot.Core.Migrations
{
    public partial class MultiPlatform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("UserId", "Accounts", "DiscordUserId");

            migrationBuilder.CreateTable(
                name: "HolderAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    AmountMined = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AmountBruteforced = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolderAccounts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HolderAccounts");

            migrationBuilder.RenameColumn("DiscordUserId", "Accounts", "UserId");
        }
    }
}
