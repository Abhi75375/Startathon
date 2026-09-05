using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ProcurementDbContext : DbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options) { }

    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();
    public DbSet<MaterialEstimationReview> MaterialEstimationReviews => Set<MaterialEstimationReview>();
    public DbSet<MaterialEstimationReviewItem> MaterialEstimationReviewItems => Set<MaterialEstimationReviewItem>();
    public DbSet<ProcurementRequest> ProcurementRequests => Set<ProcurementRequest>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<SupplierPerformanceRecord> SupplierPerformanceRecords => Set<SupplierPerformanceRecord>();
    public DbSet<Vendor> Vendors => Set<Vendor>(); // NEW

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // MaterialEstimationReview -> MaterialRequest
    modelBuilder.Entity<MaterialRequest>()
        .HasOne(m => m.MaterialEstimationReview)
        .WithMany(r => r.MaterialRequests)
        .HasForeignKey(m => m.MaterialEstimationReviewId)
        .OnDelete(DeleteBehavior.Restrict);


    // MaterialRequest -> ProcurementRequest
    modelBuilder.Entity<ProcurementRequest>()
        .HasOne(p => p.MaterialRequest)
        .WithMany(m => m.ProcurementRequests)
        .HasForeignKey(p => p.MaterialRequestId)
        .OnDelete(DeleteBehavior.Restrict);


    // ProcurementRequest -> PurchaseOrder
    modelBuilder.Entity<PurchaseOrder>()
        .HasOne(p => p.ProcurementRequest)
        .WithOne(r => r.PurchaseOrder)
        .HasForeignKey<PurchaseOrder>(p => p.ProcurementRequestId)
        .OnDelete(DeleteBehavior.Restrict);


    // MaterialRequest -> PurchaseOrder
    modelBuilder.Entity<PurchaseOrder>()
        .HasOne(p => p.MaterialRequest)
        .WithMany(m => m.PurchaseOrders)
        .HasForeignKey(p => p.MaterialRequestId)
        .OnDelete(DeleteBehavior.Restrict);


    // Vendor seed data
    modelBuilder.Entity<Vendor>().HasData(
        new Vendor
        {
            Id = "SUP-001",
            Name = "ABC Traders",
            MaterialCode = "CEMENT-001",
            PricePerUnit = 12.50m,
            DeliveryDays = 15,
            ReliabilityScore = 0.92m,
            Rating = 4.5m,
            ChatId = null
        },
        new Vendor
        {
            Id = "SUP-002",
            Name = "FastBuild Supplies",
            MaterialCode = "CEMENT-001",
            PricePerUnit = 14.00m,
            DeliveryDays = 10,
            ReliabilityScore = 0.85m,
            Rating = 4.2m,
            ChatId = null
        },
        new Vendor
        {
            Id = "SUP-003",
            Name = "CheapCo",
            MaterialCode = "CEMENT-001",
            PricePerUnit = 9.00m,
            DeliveryDays = 45,
            ReliabilityScore = 0.70m,
            Rating = 3.5m,
            ChatId = null
        },
        new Vendor
        {
            Id = "SUP-004",
            Name = "Premium Materials Inc",
            MaterialCode = "CEMENT-001",
            PricePerUnit = 25.00m,
            DeliveryDays = 5,
            ReliabilityScore = 0.98m,
            Rating = 4.9m,
            ChatId = null
        },
        new Vendor
        {
            Id = "SUP-005",
            Name = "Reliable Cement Co",
            MaterialCode = "CEMENT-001",
            PricePerUnit = 13.00m,
            DeliveryDays = 12,
            ReliabilityScore = 0.88m,
            Rating = 4.3m,
            ChatId = null
        }
    );
}
}