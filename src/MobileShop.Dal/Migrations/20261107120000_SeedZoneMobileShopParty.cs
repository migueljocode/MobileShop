#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class SeedZoneMobileShopParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "People" ("FirstName", "LastName", "PhoneNumber", "Notes", "IsDeleted")
                SELECT 'Zone', 'Mobile', '09115009109', 'The shop', 0
                WHERE NOT EXISTS (
                    SELECT 1 FROM "People"
                    WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile' AND "IsDeleted" = 0);
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Sellers" ("PersonId", "EntityType", "IsDeleted")
                SELECT p."Id", 1, 0
                FROM "People" p
                WHERE p."FirstName" = 'Zone' AND p."LastName" = 'Mobile' AND p."IsDeleted" = 0
                  AND NOT EXISTS (SELECT 1 FROM "Sellers" s WHERE s."PersonId" = p."Id");
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Customers" ("PersonId", "NationalId", "IsDeleted")
                SELECT p."Id", '0099999999', 0
                FROM "People" p
                WHERE p."FirstName" = 'Zone' AND p."LastName" = 'Mobile' AND p."IsDeleted" = 0
                  AND NOT EXISTS (SELECT 1 FROM "Customers" c WHERE c."PersonId" = p."Id");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Customers"
                WHERE "PersonId" IN (SELECT "Id" FROM "People" WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile')
                  AND NOT EXISTS (SELECT 1 FROM "Transactions" WHERE "Transactions"."CustomerId" = "Customers"."Id");
                """);

            migrationBuilder.Sql("""
                DELETE FROM "Sellers"
                WHERE "PersonId" IN (SELECT "Id" FROM "People" WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile')
                  AND NOT EXISTS (SELECT 1 FROM "Transactions" WHERE "Transactions"."SellerId" = "Sellers"."Id");
                """);

            migrationBuilder.Sql("""
                DELETE FROM "People"
                WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile'
                  AND NOT EXISTS (SELECT 1 FROM "Sellers" WHERE "Sellers"."PersonId" = "People"."Id")
                  AND NOT EXISTS (SELECT 1 FROM "Customers" WHERE "Customers"."PersonId" = "People"."Id")
                  AND NOT EXISTS (SELECT 1 FROM "Employees" WHERE "Employees"."PersonId" = "People"."Id");
                """);
        }
    }
}
