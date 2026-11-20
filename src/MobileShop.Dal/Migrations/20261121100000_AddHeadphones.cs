using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadphones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Headphones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Connection = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiseCancelling = table.Column<bool>(type: "INTEGER", nullable: false),
                    Connector = table.Column<int>(type: "INTEGER", nullable: true),
                    HasMicrophone = table.Column<bool>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headphones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Headphones_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Headphones_ProductId",
                table: "Headphones",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Headphones");
        }
    }
}
