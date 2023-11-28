namespace Tgs.Payment.GanyoPay;
public record GanyoPayClientQueryBalanceResponse(
    decimal AvailableBalance,
    decimal FrozenBalance);
