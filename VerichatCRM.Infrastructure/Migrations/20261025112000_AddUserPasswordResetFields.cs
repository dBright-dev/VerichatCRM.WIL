using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261025112000_AddUserPasswordResetFields")]
public partial class AddUserPasswordResetFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PasswordResetToken", table: "Users",
            type: "nvarchar(256)", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "PasswordResetExpiry", table: "Users",
            type: "datetime2", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PasswordResetToken", table: "Users");
        migrationBuilder.DropColumn(name: "PasswordResetExpiry", table: "Users");
    }
}
