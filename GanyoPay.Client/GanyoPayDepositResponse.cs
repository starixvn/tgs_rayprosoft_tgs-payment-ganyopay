namespace Tgs.Payment.GanyoPay.Client;
public record GanyoPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);
