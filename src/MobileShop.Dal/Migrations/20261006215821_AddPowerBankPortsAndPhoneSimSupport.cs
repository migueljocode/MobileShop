using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPowerBankPortsAndPhoneSimSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SupportsDualSim",
                table: "Phones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsEsim",
                table: "Phones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PowerBankPorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PowerBankId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Connector = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBankPorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerBankPorts_PowerBanks_PowerBankId",
                        column: x => x.PowerBankId,
                        principalTable: "PowerBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerBankPorts_PowerBankId_PortNumber",
                table: "PowerBankPorts",
                columns: new[] { "PowerBankId", "PortNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerBankPorts");

            migrationBuilder.DropColumn(
                name: "SupportsDualSim",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "SupportsEsim",
                table: "Phones");
        }
    }
}
