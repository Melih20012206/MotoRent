using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RentalListingId = table.Column<int>(type: "int", nullable: false),
                    RenterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalRequests_AspNetUsers_RenterId",
                        column: x => x.RenterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalRequests_RentalListings_RentalListingId",
                        column: x => x.RentalListingId,
                        principalTable: "RentalListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalRequests_RentalListingId",
                table: "RentalRequests",
                column: "RentalListingId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalRequests_RenterId",
                table: "RentalRequests",
                column: "RenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalRequests");
        }
    }
}
