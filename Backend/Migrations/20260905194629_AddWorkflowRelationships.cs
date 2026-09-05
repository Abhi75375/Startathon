using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaterialEstimationReviewId",
                table: "MaterialRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_MaterialRequestId",
                table: "PurchaseOrders",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ProcurementRequestId",
                table: "PurchaseOrders",
                column: "ProcurementRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_MaterialRequestId",
                table: "ProcurementRequests",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_MaterialEstimationReviewId",
                table: "MaterialRequests",
                column: "MaterialEstimationReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialRequests_MaterialEstimationReviews_MaterialEstimati~",
                table: "MaterialRequests",
                column: "MaterialEstimationReviewId",
                principalTable: "MaterialEstimationReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcurementRequests_MaterialRequests_MaterialRequestId",
                table: "ProcurementRequests",
                column: "MaterialRequestId",
                principalTable: "MaterialRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_MaterialRequests_MaterialRequestId",
                table: "PurchaseOrders",
                column: "MaterialRequestId",
                principalTable: "MaterialRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_ProcurementRequests_ProcurementRequestId",
                table: "PurchaseOrders",
                column: "ProcurementRequestId",
                principalTable: "ProcurementRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialRequests_MaterialEstimationReviews_MaterialEstimati~",
                table: "MaterialRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementRequests_MaterialRequests_MaterialRequestId",
                table: "ProcurementRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_MaterialRequests_MaterialRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_ProcurementRequests_ProcurementRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_MaterialRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_ProcurementRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_MaterialRequestId",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaterialRequests_MaterialEstimationReviewId",
                table: "MaterialRequests");

            migrationBuilder.DropColumn(
                name: "MaterialEstimationReviewId",
                table: "MaterialRequests");
        }
    }
}
