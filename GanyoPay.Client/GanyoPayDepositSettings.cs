namespace Tgs.Payment.GanyoPay.Client;
public record GanyoPayDepositSettings(
    string CustomerId,
    string PrivateKey,
    Guid FormId,
    string CustomData,
    string? PhoneNumber,
    string Channel);
