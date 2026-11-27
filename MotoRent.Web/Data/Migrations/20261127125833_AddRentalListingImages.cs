using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalListingImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentalListingImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RentalListingId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalListingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalListingImages_RentalListings_RentalListingId",
                        column: x => x.RentalListingId,
                        principalTable: "RentalListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalListingImages_RentalListingId",
                table: "RentalListingImages",
                column: "RentalListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalListingImages");
        }
    }
}
