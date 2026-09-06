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
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var useFakeInventory = builder.Configuration.GetValue<bool>("ErpSettings:UseFake");

if (useFakeInventory)
{
    builder.Services.AddScoped<IInventoryService, FakeInventoryService>();
}
else
{
    builder.Services.AddHttpClient<IInventoryService, ErpInventoryService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ErpSettings:BaseUrl"]!);
    });
}

builder.Services.AddScoped<InventoryCheckService>();

builder.Services.AddHttpClient<IProjectDataService, ProjectDataService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProjectApiSettings:BaseUrl"]!
    );
});

builder.Services.AddScoped<IHistoricalProjectDataService, FakeHistoricalProjectDataService>();
builder.Services.AddScoped<MaterialEstimationService>();builder.Services.AddScoped<IHistoricalProjectDataService, FakeHistoricalProjectDataService>();
builder.Services.AddScoped<MaterialEstimationService>();

builder.Services.AddScoped<ISupervisorReviewGateway, FakeSupervisorReviewGateway>();
builder.Services.AddScoped<MaterialEstimationReviewService>();

builder.Services.AddScoped<ISupplierService, VendorService>();
builder.Services.AddScoped<IBudgetService, FakeBudgetService>();
builder.Services.AddScoped<SupplierSelectionService>();

builder.Services.AddScoped<ProcurementRequestService>();
builder.Services.AddScoped<IProcurementApprovalGateway, FakeProcurementApprovalGateway>();
builder.Services.AddHttpClient<IWhatsAppService,MetaWhatsAppService>();
builder.Services.AddHttpClient<ITelegramService, TelegramService>();

builder.Services.AddScoped<IPoApprovalGateway, FakePoApprovalGateway>();
builder.Services.AddScoped<PurchaseOrderService>();

builder.Services.AddScoped<IProcurementWorkflowService, ProcurementWorkflowService>();

builder.Services.AddScoped<IVendorReplyParser, VendorReplyParser>();

var useFakeNotifications = builder.Configuration.GetValue<bool>("NotificationSettings:UseFake");

if (useFakeNotifications)
{
    builder.Services.AddScoped<IOrderNotificationGateway, FakeOrderNotificationGateway>();
}
else
{
    builder.Services.AddScoped<IOrderNotificationGateway, TelegramOrderNotificationGateway>();
}
builder.Services.AddScoped<DeliveryTrackingService>();


var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
//app.UseHttpsRedirection();
app.MapControllers();
app.Run();