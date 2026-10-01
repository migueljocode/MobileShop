using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPartNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartNumberId",
                table: "Phones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SupportsDualSim = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsEsim = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartNumbers_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Phones_PartNumberId",
                table: "Phones",
                column: "PartNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartNumbers_ModelId_Code",
                table: "PartNumbers",
                columns: new[] { "ModelId", "Code" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Phones_PartNumbers_PartNumberId",
                table: "Phones",
                column: "PartNumberId",
                principalTable: "PartNumbers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phones_PartNumbers_PartNumberId",
                table: "Phones");

            migrationBuilder.DropTable(
                name: "PartNumbers");

            migrationBuilder.DropIndex(
                name: "IX_Phones_PartNumberId",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "PartNumberId",
                table: "Phones");
        }
    }
}
