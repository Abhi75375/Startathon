using Backend.Data;
using Backend.Contracts;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Backend.Services.Telegram;
using Backend.Services.WhatsApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProcurementDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));


// ======================================================
// INVENTORY
// ======================================================

var useFakeInventory =
    builder.Configuration.GetValue<bool>(
        "ErpSettings:UseFake");

if (useFakeInventory)
{
    builder.Services.AddScoped<
        IInventoryService,
        FakeInventoryService>();
}
else
{
    builder.Services.AddHttpClient<
        IInventoryService,
        ErpInventoryService>(
        client =>
        {
            client.BaseAddress =
                new Uri(
                    builder.Configuration[
                        "ErpSettings:BaseUrl"]!);
        });
}

builder.Services.AddScoped<
    InventoryCheckService>();


// ======================================================
// PROJECT + MATERIAL ESTIMATION
// ======================================================

builder.Services.AddScoped<
    IProjectDataService,
    FakeProjectDataService>();

builder.Services.AddScoped<
    IHistoricalProjectDataService,
    FakeHistoricalProjectDataService>();

builder.Services.AddScoped<
    MaterialEstimationService>();


// ======================================================
// FRONTEND MATERIAL ESTIMATION DELIVERY
// ======================================================

builder.Services.AddHttpClient<
    IMaterialEstimationFrontendGateway,
    MaterialEstimationFrontendGateway>(
    client =>
    {
        var baseUrl =
            builder.Configuration[
                "FrontendSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "FrontendSettings:BaseUrl is not configured.");
        }

        client.BaseAddress =
            new Uri(baseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<
    MaterialEstimationReviewService>();


// ======================================================
// SUPPLIERS
// ======================================================

builder.Services.AddScoped<
    ISupplierService,
    VendorService>();

builder.Services.AddScoped<
    IBudgetService,
    FakeBudgetService>();

builder.Services.AddScoped<
    SupplierSelectionService>();


// ======================================================
// PROCUREMENT REQUEST
// ======================================================

builder.Services.AddScoped<
    ProcurementRequestService>();

builder.Services.AddScoped<
    IProcurementApprovalGateway,
    FakeProcurementApprovalGateway>();


builder.Services.AddHttpClient<IVendorApprovalGateway, ExternalVendorApprovalGateway>();

builder.Services.AddScoped<VendorApprovalService>();
builder.Services.AddScoped<IVendorApprovalGateway, ExternalVendorApprovalGateway>();
builder.Services.AddHttpClient<IVendorApprovalGateway, ExternalVendorApprovalGateway>();
builder.Services.AddScoped<VendorApprovalService>();


// ======================================================
// COMMUNICATION
// ======================================================

builder.Services.AddHttpClient<
    IWhatsAppService,
    MetaWhatsAppService>();

builder.Services.AddHttpClient<
    ITelegramService,
    TelegramService>();


// ======================================================
// PURCHASE ORDER
// ======================================================

builder.Services.AddScoped<
    IPoApprovalGateway,
    FakePoApprovalGateway>();

builder.Services.AddScoped<
    PurchaseOrderService>();

builder.Services.AddScoped<
    IProcurementWorkflowService,
    ProcurementWorkflowService>();


// ======================================================
// TELEGRAM VENDOR RESPONSE
// ======================================================

builder.Services.AddScoped<
    IVendorReplyParser,
    VendorReplyParser>();

var useFakeNotifications =
    builder.Configuration.GetValue<bool>(
        "NotificationSettings:UseFake");

if (useFakeNotifications)
{
    builder.Services.AddScoped<
        IOrderNotificationGateway,
        FakeOrderNotificationGateway>();
}
else
{
    builder.Services.AddScoped<
        IOrderNotificationGateway,
        TelegramOrderNotificationGateway>();
}

builder.Services.AddScoped<
    DeliveryTrackingService>();


// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();