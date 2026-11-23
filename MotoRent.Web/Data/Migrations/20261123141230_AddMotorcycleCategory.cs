using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMotorcycleCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "RentalListings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "RentalListings");
        }
    }
}
