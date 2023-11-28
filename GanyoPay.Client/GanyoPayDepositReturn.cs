namespace Tgs.Payment.GanyoPay.Client;
public record GanyoPayDepositReturn
{
    public string ThirdPartyOrderNo { get; set; } = default!;

    public decimal OrderAmount { get; set; }

    public decimal RealAmount { get; set; }

    public Guid FormId { get; set; }

    public string Status { get; set; } = default!;

    public string Sign { get; set; } = default!;
}
