namespace Tgs.Payment.GanyoPay;
public record GanyoPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);
