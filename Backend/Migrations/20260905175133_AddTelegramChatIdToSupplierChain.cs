using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramChatIdToSupplierChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierTelegramChatId",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierTelegramChatId",
                table: "ProcurementRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedSupplierTelegramChatId",
                table: "MaterialRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierTelegramChatId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SupplierTelegramChatId",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "SelectedSupplierTelegramChatId",
                table: "MaterialRequests");
        }
    }
}
