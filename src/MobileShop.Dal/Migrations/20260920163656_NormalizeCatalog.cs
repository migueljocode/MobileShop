using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IPhones");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Phones");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SecondHands",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Phones",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Guarantees",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppleInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatteryHealth = table.Column<int>(type: "INTEGER", nullable: false),
                    ChargeCycle = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppleInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppleInfos_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Connector1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Connector2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cables_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chargers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Wattage = table.Column<int>(type: "INTEGER", nullable: false),
                    Pd = table.Column<bool>(type: "INTEGER", nullable: false),
                    PortCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chargers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chargers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Glasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Glasses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Laptops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cpu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Gpu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplaySize = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laptops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Laptops_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerBanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CapacityMah = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxWattage = table.Column<int>(type: "INTEGER", nullable: false),
                    PortCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Pd = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerBanks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmartWatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartWatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmartWatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StorageCapacities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Gb = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageCapacities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tablets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tablets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tablets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ManufacturerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModelNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Models_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ram = table.Column<int>(type: "INTEGER", nullable: false),
                    StorageCapacityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChargeSpeed = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSpecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSpecs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceSpecs_StorageCapacities_StorageCapacityId",
                        column: x => x.StorageCapacityId,
                        principalTable: "StorageCapacities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortableStorages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    StorageCapacityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Speed = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortableStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortableStorages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PortableStorages_StorageCapacities_StorageCapacityId",
                        column: x => x.StorageCapacityId,
                        principalTable: "StorageCapacities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CaseModelFits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseModelFits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseModelFits_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CaseModelFits_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GlassModelFits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GlassId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassModelFits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlassModelFits_Glasses_GlassId",
                        column: x => x.GlassId,
                        principalTable: "Glasses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GlassModelFits_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ColorId",
                table: "Products",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ModelId",
                table: "Products",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppleInfos_ProductId",
                table: "AppleInfos",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cables_ProductId",
                table: "Cables",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseModelFits_CaseId_ModelId",
                table: "CaseModelFits",
                columns: new[] { "CaseId", "ModelId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CaseModelFits_ModelId",
                table: "CaseModelFits",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ProductId",
                table: "Cases",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chargers_ProductId",
                table: "Chargers",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSpecs_ProductId",
                table: "DeviceSpecs",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSpecs_StorageCapacityId",
                table: "DeviceSpecs",
                column: "StorageCapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_Glasses_ProductId",
                table: "Glasses",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlassModelFits_GlassId_ModelId",
                table: "GlassModelFits",
                columns: new[] { "GlassId", "ModelId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GlassModelFits_ModelId",
                table: "GlassModelFits",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Laptops_ProductId",
                table: "Laptops",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Name",
                table: "Manufacturers",
                column: "Name",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CategoryId",
                table: "Models",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_ManufacturerId_Name",
                table: "Models",
                columns: new[] { "ManufacturerId", "Name" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PortableStorages_ProductId",
                table: "PortableStorages",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortableStorages_StorageCapacityId",
                table: "PortableStorages",
                column: "StorageCapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBanks_ProductId",
                table: "PowerBanks",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmartWatches_ProductId",
                table: "SmartWatches",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageCapacities_Gb",
                table: "StorageCapacities",
                column: "Gb",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tablets_ProductId",
                table: "Tablets",
                column: "ProductId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Colors_ColorId",
                table: "Products",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Models_ModelId",
                table: "Products",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Colors_ColorId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Models_ModelId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "AppleInfos");

            migrationBuilder.DropTable(
                name: "Cables");

            migrationBuilder.DropTable(
                name: "CaseModelFits");

            migrationBuilder.DropTable(
                name: "Chargers");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "DeviceSpecs");

            migrationBuilder.DropTable(
                name: "GlassModelFits");

            migrationBuilder.DropTable(
                name: "Laptops");

            migrationBuilder.DropTable(
                name: "PortableStorages");

            migrationBuilder.DropTable(
                name: "PowerBanks");

            migrationBuilder.DropTable(
                name: "SmartWatches");

            migrationBuilder.DropTable(
                name: "Tablets");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Glasses");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "StorageCapacities");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ColorId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ModelId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SecondHands");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Guarantees");

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Phones",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IPhones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatteryHealth = table.Column<int>(type: "INTEGER", nullable: false),
                    ChargeCycle = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IPhones_Phones_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "Phones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IPhones_PhoneId",
                table: "IPhones",
                column: "PhoneId",
                unique: true);
        }
    }
}
