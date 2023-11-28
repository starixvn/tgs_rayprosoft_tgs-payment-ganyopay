using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;

namespace Tgs.Payment.GanyoPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    GanyoPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);
