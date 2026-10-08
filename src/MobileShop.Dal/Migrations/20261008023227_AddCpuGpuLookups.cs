using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddCpuGpuLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Cpus", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Gpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Gpus", x => x.Id));

            migrationBuilder.Sql("""
                INSERT INTO "Cpus" ("Name", "IsDeleted")
                SELECT DISTINCT "Cpu", 0 FROM "Laptops";
                INSERT INTO "Gpus" ("Name", "IsDeleted")
                SELECT DISTINCT "Gpu", 0 FROM "Laptops" WHERE trim("Gpu") <> '';
                """);

            migrationBuilder.AddColumn<int>(
                name: "CpuId",
                table: "Laptops",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GpuId",
                table: "Laptops",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Laptops"
                SET "CpuId" = (SELECT "Id" FROM "Cpus" WHERE "Cpus"."Name" = "Laptops"."Cpu" LIMIT 1);
                UPDATE "Laptops"
                SET "GpuId" = (SELECT "Id" FROM "Gpus" WHERE "Gpus"."Name" = "Laptops"."Gpu" LIMIT 1)
                WHERE trim("Laptops"."Gpu") <> '';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CpuId",
                table: "Laptops",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.DropColumn(name: "Cpu", table: "Laptops");
            migrationBuilder.DropColumn(name: "Gpu", table: "Laptops");

            migrationBuilder.CreateIndex(name: "IX_Laptops_CpuId", table: "Laptops", column: "CpuId");
            migrationBuilder.CreateIndex(name: "IX_Laptops_GpuId", table: "Laptops", column: "GpuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Laptops_Cpus_CpuId",
                table: "Laptops",
                column: "CpuId",
                principalTable: "Cpus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Laptops_Gpus_GpuId",
                table: "Laptops",
                column: "GpuId",
                principalTable: "Gpus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Laptops_Cpus_CpuId", table: "Laptops");
            migrationBuilder.DropForeignKey(name: "FK_Laptops_Gpus_GpuId", table: "Laptops");
            migrationBuilder.DropIndex(name: "IX_Laptops_CpuId", table: "Laptops");
            migrationBuilder.DropIndex(name: "IX_Laptops_GpuId", table: "Laptops");

            migrationBuilder.AddColumn<string>(
                name: "Cpu",
                table: "Laptops",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gpu",
                table: "Laptops",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Laptops"
                SET "Cpu" = (SELECT "Name" FROM "Cpus" WHERE "Cpus"."Id" = "Laptops"."CpuId");
                UPDATE "Laptops"
                SET "Gpu" = COALESCE(
                    (SELECT "Name" FROM "Gpus" WHERE "Gpus"."Id" = "Laptops"."GpuId"),
                    '');
                """);

            migrationBuilder.DropColumn(name: "CpuId", table: "Laptops");
            migrationBuilder.DropColumn(name: "GpuId", table: "Laptops");
            migrationBuilder.DropTable(name: "Cpus");
            migrationBuilder.DropTable(name: "Gpus");
        }
    }
}
