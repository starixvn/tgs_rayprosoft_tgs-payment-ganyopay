using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;

namespace Tgs.Payment.GanyoPay;
public record WithdrawForm(
    Guid Id,
    string? UserId,
    UserType UserType,
    Guid BrandId,
    Guid? BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    Guid BankInfoId,
    GanyoPayWithdrawApplyInfo? ApplyInfo,
    string? PaymentReturn = null);
