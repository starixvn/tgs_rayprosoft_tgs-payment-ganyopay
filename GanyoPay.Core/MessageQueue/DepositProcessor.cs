using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Valhalla.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;
using Tgs.Payment.Protos;

namespace Tgs.Payment.GanyoPay.MessageQueue;

internal class DepositProcessor : IMessageProcessor
{
    private readonly GanyoPayClient _ganyoPayClient;
    private readonly IPaymentClient _paymentClient;
    private readonly GanyoPayOptions _options;

    public DepositProcessor(
        GanyoPayClient ganyoPayClient,
        IPaymentClient paymentClient,
        IOptions<GanyoPayOptions> optionsAccessor)
    {
        _ganyoPayClient = ganyoPayClient ?? throw new ArgumentNullException(nameof(ganyoPayClient));
        _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        _options = optionsAccessor.Value;
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = DepositRequest.Parser.ParseFrom(data.Span);

        var settings = JsonSerializer.Deserialize<GanyoPayDepositSettings>(
            request.ApplyInfo,
            GanyoPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(request.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.CustomData,
            settings.PhoneNumber,
            settings.Channel);

        var depositResponse = await _ganyoPayClient.DepositAsync(
            settings.CustomerId,
            settings.PrivateKey,
            order,
            cancellationToken).ConfigureAwait(false);

        var applyInfo = new GanyoPayDepositApplyInfo(
            settings,
            new GanyoPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        await _paymentClient.ScheduleAsync(
            "tgs.payment.ganyopay.deposit.schedule.reject",
            settings.FormId,
            DateTime.UtcNow.AddMinutes(31D),
            cancellationToken).ConfigureAwait(false);

        var msg = new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
