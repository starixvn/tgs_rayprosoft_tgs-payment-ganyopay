namespace Tgs.Payment.GanyoPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string AccountNumber,
    string AccountName,
    string BankName,
    string? Channel);
