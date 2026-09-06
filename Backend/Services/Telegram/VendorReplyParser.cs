namespace Backend.Services.Telegram;

public record VendorReply(
    bool CanSupply,
    decimal? AvailableQuantity);

public interface IVendorReplyParser
{
    VendorReply? Parse(string text);
}

public class VendorReplyParser : IVendorReplyParser
{
    public VendorReply? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var parts = text
            .Trim()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return null;
        }

        var command = parts[0]
            .Trim()
            .ToUpperInvariant();

        switch (command)
        {
            case "YES":
            {
                decimal? availableQuantity = null;

                // "YES" means vendor can provide the full quantity.
                //
                // "YES 40" means vendor says they can provide
                // 40 units.

                if (parts.Length > 1)
                {
                    if (!decimal.TryParse(
                            parts[1],
                            out var parsedQuantity))
                    {
                        return null;
                    }

                    if (parsedQuantity < 0)
                    {
                        return null;
                    }

                    availableQuantity = parsedQuantity;
                }

                return new VendorReply(
                    CanSupply: true,
                    AvailableQuantity: availableQuantity);
            }

            case "NO":
            {
                return new VendorReply(
                    CanSupply: false,
                    AvailableQuantity: null);
            }

            default:
                return null;
        }
    }
}