using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AVStack.IdentityServer.WebApi.Data.Migrations.Identity
{
    public partial class UserTempTokenTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AVUserTempToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2147483647, nullable: false),
                    TokenType = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AVUserTempToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AVUserTempTable_AVUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AVUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AVUserTempToken");
        }
    }
}