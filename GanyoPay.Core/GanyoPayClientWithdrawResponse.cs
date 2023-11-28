namespace Tgs.Payment.GanyoPay;
public record GanyoPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);
