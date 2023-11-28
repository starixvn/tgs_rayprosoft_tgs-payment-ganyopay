namespace Tgs.Payment.GanyoPay.Client;
public record GanyoPayWithdrawSettings(
    string CustomerId,
    string PrivateKey,
    Guid FormId,
    string AccountNumber,
    string AccountName,
    string BankName,
    string? Channel);
