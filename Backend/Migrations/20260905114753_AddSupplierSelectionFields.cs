using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierSelectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "MaterialRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "MaterialRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedSupplierId",
                table: "MaterialRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedSupplierName",
                table: "MaterialRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SelectedSupplierPrice",
                table: "MaterialRequests",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShortageQuantity",
                table: "MaterialRequests",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "SelectedSupplierId",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "SelectedSupplierName",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "SelectedSupplierPrice",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "ShortageQuantity",
                table: "MaterialRequests");
        }
    }
}
