using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations;

public partial class UseIntegerRialMoney : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing databases may contain legacy fractional values. Round them once before
        // changing the columns to integer Rial amounts.
        migrationBuilder.Sql("UPDATE \"Products\" SET \"Price\" = ROUND(\"Price\");");
        migrationBuilder.Sql("UPDATE \"Transactions\" SET \"FinishedPrice\" = ROUND(\"FinishedPrice\");");

        migrationBuilder.AlterColumn<int>(
            name: "Price",
            table: "Products",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)");

        migrationBuilder.AlterColumn<int>(
            name: "FinishedPrice",
            table: "Transactions",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "Products",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<decimal>(
            name: "FinishedPrice",
            table: "Transactions",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");
    }
}