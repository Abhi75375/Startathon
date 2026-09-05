using Backend.Data;
using Backend.Contracts;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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
builder.Services.AddScoped<IProjectDataService, FakeProjectDataService>();
builder.Services.AddScoped<IHistoricalProjectDataService, FakeHistoricalProjectDataService>();
builder.Services.AddScoped<MaterialEstimationService>();
builder.Services.AddScoped<ISupervisorReviewGateway, FakeSupervisorReviewGateway>();
builder.Services.AddScoped<MaterialEstimationReviewService>();
builder.Services.AddScoped<ISupplierService, FakeSupplierService>();
builder.Services.AddScoped<IBudgetService, FakeBudgetService>();
builder.Services.AddScoped<SupplierSelectionService>();
builder.Services.AddScoped<ProcurementRequestService>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();