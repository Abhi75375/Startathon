using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialEstimationReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialEstimationReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialEstimationReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialEstimationReviewItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialEstimationReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    AiEstimatedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    Approved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialEstimationReviewItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialEstimationReviewItems_MaterialEstimationReviews_Mat~",
                        column: x => x.MaterialEstimationReviewId,
                        principalTable: "MaterialEstimationReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialEstimationReviewItems_MaterialEstimationReviewId",
                table: "MaterialEstimationReviewItems",
                column: "MaterialEstimationReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialEstimationReviewItems");

            migrationBuilder.DropTable(
                name: "MaterialEstimationReviews");
        }
    }
}
