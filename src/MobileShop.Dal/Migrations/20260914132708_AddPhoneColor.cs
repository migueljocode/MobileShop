using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Phones",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Phones");
        }
    }
}
