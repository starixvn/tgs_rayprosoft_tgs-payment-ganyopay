namespace Tgs.Payment.GanyoPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string CustomData,
    string? PhoneNumber,
    string Channel);
