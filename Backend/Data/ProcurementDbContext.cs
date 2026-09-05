using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ProcurementDbContext : DbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options) { }

    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();
    public DbSet<MaterialEstimationReview> MaterialEstimationReviews => Set<MaterialEstimationReview>();
    public DbSet<MaterialEstimationReviewItem> MaterialEstimationReviewItems => Set<MaterialEstimationReviewItem>();
}