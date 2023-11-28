using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Valhalla.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;
using Tgs.Payment.Protos;

namespace Tgs.Payment.GanyoPay.MessageQueue;

internal class WithdrawProcessor : IMessageProcessor
{
    private readonly GanyoPayClient _ganyoPayClient;
    private readonly IPaymentClient _paymentClient;
    private readonly GanyoPayOptions _options;

    public WithdrawProcessor(
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
        var request = WithdrawRequest.Parser.ParseFrom(data.Span);
        var settings = JsonSerializer.Deserialize<GanyoPayWithdrawSettings>(request.ApplyInfo, GanyoPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(request.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.AccountNumber,
            settings.AccountName,
            settings.BankName,
            settings.Channel);

        var withdrawResponse = await _ganyoPayClient.WithdrawAsync(
            settings.CustomerId,
            settings.PrivateKey,
            order,
            cancellationToken).ConfigureAwait(false);
        var applyInfo = new GanyoPayWithdrawApplyInfo(
            settings,
            new GanyoPayWithdrawResponse(withdrawResponse.FormId));

        await _paymentClient.SaveWithdrawFormThirdPartyOrderNoAsync(
            settings.FormId,
            withdrawResponse.ThirdPartyOrderNo,
            cancellationToken);

        var msg = new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
