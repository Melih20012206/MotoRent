using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteListings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RentalListingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteListings", x => new { x.UserId, x.RentalListingId });
                    table.ForeignKey(
                        name: "FK_FavoriteListings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteListings_RentalListings_RentalListingId",
                        column: x => x.RentalListingId,
                        principalTable: "RentalListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteListings_RentalListingId",
                table: "FavoriteListings",
                column: "RentalListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteListings");
        }
    }
}
