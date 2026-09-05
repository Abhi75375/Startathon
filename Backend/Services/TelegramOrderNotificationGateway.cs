using Backend.Contracts;
using Backend.Services.Telegram;

namespace Backend.Services;

public class TelegramOrderNotificationGateway : IOrderNotificationGateway
{
    private readonly ITelegramService _telegramService;

    public TelegramOrderNotificationGateway(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    public async Task SendOrderAsync(OrderNotificationPayload payload)
    {
        var message = $"📦 New Order - PO {payload.PoNumber}\n" +
                      $"Material: {payload.MaterialCode}\n" +
                      $"Quantity: {payload.Quantity}\n" +
                      $"Total: ${payload.TotalCost}\n" +
                      $"Expected delivery: {payload.EstimatedDeliveryDate:yyyy-MM-dd}";

        await _telegramService.SendMessageAsync(payload.SupplierTelegramChatId, message);
    }
}