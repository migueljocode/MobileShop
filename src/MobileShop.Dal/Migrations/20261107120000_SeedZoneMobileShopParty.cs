#nullable disable

namespace MobileShop.Dal.Migrations
{
    /// <summary>
    /// Intentionally empty. The Zone Mobile shop party is ensured at startup from <c>Pdf:ShopPhone</c>
    /// (see <c>ShopPartyDataService</c>) because a migration cannot read configuration. Kept so the
    /// migration history stays valid on databases where it may already have been applied.
    /// </summary>
    public partial class SeedZoneMobileShopParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
