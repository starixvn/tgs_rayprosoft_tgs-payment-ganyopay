using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay;
using Tgs.Payment.GanyoPay.Client;
using Tgs.Payment.GanyoPay.MessageQueue;
using Tgs.Payment.Protos;

namespace GanyoPay.Core.UnitTests;
public class WithdrawProcessorTests
{
    [Fact]
    public async Task 處理提款要求()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var fakeGanyoPayClient = new GanyoPayClient(
            httpClient,
            new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance),
            NullLogger<GanyoPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new GanyoPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/ganyopay"),
            PaymentApiUrl = new Uri("https://www.enier13.com"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakeGanyoPayClient,
            fakePaymentClient,
            new OptionsWrapper<GanyoPayOptions>(options));

        var formId = Guid.Parse("4ebdabc9-ce7b-4639-88f9-56eadfb7b999");
        var customerId = "taiwan";
        var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

        var applySettings = new GanyoPayWithdrawSettings(
            customerId,
            privateKey,
            formId,
            AccountNumber: "testAccountNumber",
            AccountName: "testAccountName",
            BankName: "bkash",
            null);

        var request = new WithdrawRequest
        {
            Amount = "1000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, GanyoPayUtility.SerializerOptions)
        };

        _ = mockHttp
            .Expect(HttpMethod.Post, "https://www.enier13.com/api/pay_out")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            })
            .WithContent(await new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer_id", "taiwan" },
                { "amount", "1000" },
                { "reference_number","4ebdabc9-ce7b-4639-88f9-56eadfb7b999" },
                { "backend_url","https://dev-payment.rpgp.cc/ganyopay/withdraw/callback" },
                { "account_number", "testAccountNumber" },
                { "account_name", "testAccountName" },
                { "bank_name", "bkash" },
                { "token", "8Wgzy7QF+Ee6Pxr4Xx9rNWdc0eJUv0GZUDNtYpDOawY=" }
            }).ReadAsStringAsync().ConfigureAwait(false))
            .Respond(HttpStatusCode.OK, new StringContent(
                JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        customer_id = "taiwan",
                        amount = "1000.00",
                        reference_number = "4ebdabc9-ce7b-4639-88f9-56eadfb7b999",
                        fee = "25.00",
                        custom_data = "",
                        result = "waiting",
                        time_stamp = "20231127075622",
                        token = "Bv5e/yAVNoAZBzbelWANEvFi4Y7VWnsVr6ygHCEbCE0=",
                    },
                    ret_code = "SUCCESS"
                })));

        // Act
        var actual = await sut.HandleAsync(request.ToByteArray());
        var response = WithdrawResponse.Parser.ParseFrom(actual.Span);
        var applyInfo = JsonSerializer.Deserialize<GanyoPayWithdrawApplyInfo>(response.Result, GanyoPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("4ebdabc9-ce7b-4639-88f9-56eadfb7b999"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}
