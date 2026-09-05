using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTrackingAndSupplierPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDeliveryDate",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveredQuantity",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryStatus",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderedAt",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierPerformanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<string>(type: "text", nullable: false),
                    SupplierName = table.Column<string>(type: "text", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OnTime = table.Column<bool>(type: "boolean", nullable: false),
                    DaysLate = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPerformanceRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierPerformanceRecords");

            migrationBuilder.DropColumn(
                name: "ActualDeliveryDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DeliveredQuantity",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderedAt",
                table: "PurchaseOrders");
        }
    }
}
