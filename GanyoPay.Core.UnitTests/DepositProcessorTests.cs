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
public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
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
        var formId = Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d");
        var customerId = "taiwan";
        var privateKey = "e274026fbe9e6869fbf5e0350d2e42cd";

        var sut = new DepositProcessor(
            fakeGanyoPayClient,
            fakePaymentClient,
            new OptionsWrapper<GanyoPayOptions>(options));

        var applySettings = new GanyoPayDepositSettings(
            customerId,
            privateKey,
            formId,
            "4ucmqplxb029",
            null,
            "nagad");

        var request = new DepositRequest
        {
            Amount = "25000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, GanyoPayUtility.SerializerOptions)
        };

        _ = mockHttp
            .Expect(HttpMethod.Post, "https://www.enier13.com/api/pay_in")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            })
            .WithContent(await new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer_id", "taiwan" },
                { "amount", "25000" },
                { "currency","BDT" },
                { "reference_number","a2da47e3-04cc-4451-b600-279cc3a7bd8d" },
                { "custom_data", "4ucmqplxb029" },
                { "backend_url","https://dev-payment.rpgp.cc/ganyopay/deposit/callback" },
                { "channel", "nagad" },
                { "version", "1.0.0" },
                { "token", "sCeo2EYznl2grPOoWQo2WncpnCnNMihFC6w/2wgrgrw=" }
            }).ReadAsStringAsync().ConfigureAwait(false))
            .Respond(HttpStatusCode.OK, new StringContent(
                JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        customer_id = "taiwan",
                        reference_number = "a2da47e3-04cc-4451-b600-279cc3a7bd8d",
                        amount = "25000.00",
                        custom_data = "4ucmqplxb029",
                        result = "Waiting",
                        bank_name = "Nagad",
                        number = "01739547354",
                        holder_name = "MD TANZID",
                        payment_url = "https://mobile.yoyo148148.com/deposit?orderNo=1729007289152569344",
                        token = "X4Vu9x8RldSqAYBWlTVX3MVyvxMJaUV4dY563fuJn/w=",
                    },
                    ret_code = "SUCCESS"
                })));

        // Act
        var actual = await sut.HandleAsync(request.ToByteArray());
        var response = DepositResponse.Parser.ParseFrom(actual.Span);
        var applyInfo = JsonSerializer.Deserialize<GanyoPayDepositApplyInfo>(response.Result, GanyoPayUtility.SerializerOptions);

        // Assert
        //_ = fakePaymentClient.Received(1)
        //    .ScheduleAsync(
        //        Arg.Is("tgs.payment.ganyopay.deposit.schedule.reject"),
        //        Arg.Is(applySettings.FormId),
        //        Arg.Any<DateTime>());
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new GanyoPayDepositResponse(
                25000m,
                applySettings.FormId,
                new Uri("https://mobile.yoyo148148.com/deposit?orderNo=1729007289152569344"),
                "a2da47e3-04cc-4451-b600-279cc3a7bd8d"),
            applyInfo.Response);
    }
}
