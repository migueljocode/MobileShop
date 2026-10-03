using Microsoft.EntityFrameworkCore.Migrations;

namespace MobileShop.Dal.Migrations;

public partial class WidenMoneyToLong : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite stores both CLR int and long as INTEGER (64-bit), so no SQL/table rebuild is required.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // SQLite stores both CLR int and long as INTEGER (64-bit), so no SQL/table rebuild is required.
    }
}
