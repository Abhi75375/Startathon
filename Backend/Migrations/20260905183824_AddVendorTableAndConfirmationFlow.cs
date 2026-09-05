using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorTableAndConfirmationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentForConfirmationAt",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendorConfirmationStatus",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VendorConfirmedQuantity",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VendorRespondedAt",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExcludedSupplierIds",
                table: "MaterialRequests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    ChatId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "Id", "ChatId", "DeliveryDays", "MaterialCode", "Name", "PricePerUnit", "Rating", "ReliabilityScore" },
                values: new object[,]
                {
                    { "SUP-001", null, 15, "CEMENT-001", "ABC Traders", 12.50m, 4.5m, 0.92m },
                    { "SUP-002", null, 10, "CEMENT-001", "FastBuild Supplies", 14.00m, 4.2m, 0.85m },
                    { "SUP-003", null, 45, "CEMENT-001", "CheapCo", 9.00m, 3.5m, 0.70m },
                    { "SUP-004", null, 5, "CEMENT-001", "Premium Materials Inc", 25.00m, 4.9m, 0.98m },
                    { "SUP-005", null, 12, "CEMENT-001", "Reliable Cement Co", 13.00m, 4.3m, 0.88m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropColumn(
                name: "SentForConfirmationAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "VendorConfirmationStatus",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "VendorConfirmedQuantity",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "VendorRespondedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ExcludedSupplierIds",
                table: "MaterialRequests");
        }
    }
}
