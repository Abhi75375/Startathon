using Backend.Contracts;
using Backend.Data;
using Backend.Services;
using Backend.Services.Telegram;
using Backend.Services.WhatsApp;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProcurementDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "Default")));


// ======================================================
// INVENTORY
// ======================================================

// Backend/Program.cs
// ERP registration

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

            client.Timeout =
                TimeSpan.FromSeconds(30);
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
// MATERIAL ESTIMATION DELIVERY
// ======================================================

builder.Services.AddScoped<
    IMaterialEstimationFrontendGateway,
    FakeMaterialEstimationFrontendGateway>();

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
// VENDOR APPROVAL
// ======================================================

builder.Services.AddHttpClient<
    IVendorApprovalGateway,
    ExternalVendorApprovalGateway>();

builder.Services.AddScoped<
    VendorApprovalService>();


// ======================================================
// PROCUREMENT REQUEST
// ======================================================

builder.Services.AddScoped<
    ProcurementRequestService>();

builder.Services.AddScoped<
    IProcurementApprovalGateway,
    FakeProcurementApprovalGateway>();


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


// ======================================================
// MAIN WORKFLOW
// ======================================================

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
// BUILD
// ======================================================

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

app.Run();