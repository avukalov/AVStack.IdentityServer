using Microsoft.EntityFrameworkCore.Migrations;

namespace AVStack.IdentityServer.WebApi.Data.Migrations
{
    public partial class LevelPropertyOnRoleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Role",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Role");
        }
    }
}
