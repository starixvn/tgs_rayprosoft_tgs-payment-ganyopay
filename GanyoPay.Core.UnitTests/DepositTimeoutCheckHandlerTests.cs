using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay;
using Tgs.Payment.GanyoPay.Client;
using Tgs.Payment.GanyoPay.MessageQueue;
using Tgs.Payment.Protos;

namespace GanyoPay.Core.UnitTests;

public class DepositTimeoutCheckHandlerTests
{
    [Fact]
    public async Task 檢查已經申請三十一分鐘的單子是否已經完成()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var fakeSBPayClient = new GanyoPayClient(httpClient, new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance), NullLogger<GanyoPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new DepositTimeoutCheckHandler(fakePaymentServiceClient, fakeSBPayClient, fakePaymentClient, NullLogger<DepositTimeoutCheckHandler>.Instance);

        var formId = Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var request = new ScheduleRequest
        {
            FormId = formId,
            Subject = "tgs.payment.ganyopay.deposit.schedule.reject",
            ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var applyInfo = new GanyoPayDepositApplyInfo(
            new GanyoPayDepositSettings(
                "taiwan",
                "e274026fbe9e6869fbf5e0350d2e42cd",
                formId,
                "4ucmqplxb029",
                null,
                "nagad"),
            new GanyoPayDepositResponse(
                25000m,
                formId,
                new Uri("https://mobile.yoyo148148.com/deposit?orderNo=1729007289152569344"),
                "a2da47e3-04cc-4451-b600-279cc3a7bd8d"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 25000m,
                BrandId = brandId,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
            });

        _ = mockHttp
            .Expect(HttpMethod.Post, "https://www.enier13.com/api/pay_in/query")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            })
            .WithContent(await new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer_id", "taiwan" },
                { "reference_number","a2da47e3-04cc-4451-b600-279cc3a7bd8d" },
                { "token", "wKbGd8vV5eWZS2z+DbXyhRargJRXxZwg0H5qUfkoHaA=" }
            }).ReadAsStringAsync().ConfigureAwait(false))
            .Respond(HttpStatusCode.OK, new StringContent(
                JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        amount = "25000.00",
                        reference_number = "a2da47e3-04cc-4451-b600-279cc3a7bd8d",
                        fee = "1125.00",
                        custom_data = "4ucmqplxb029",
                        result = "failure",
                        token = "5H2TSJRY8s16hF74FsfDN4LegeRjr6V3iX3UPIF9WuQ=",
                    },
                    ret_code = "SUCCESS"
                })));

        // Act
        await sut.HandleAsync(request.ToByteArray());

        // Assert
        _ = fakePaymentClient
            .Received(1)
            .RejectDepositAsync(Arg.Is(formId), Arg.Is(brandId));
    }

    [Fact]
    public async Task 當三方代收結果為waiting時會繼續計時詢問()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var fakeSBPayClient = new GanyoPayClient(httpClient, new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance), NullLogger<GanyoPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new DepositTimeoutCheckHandler(fakePaymentServiceClient, fakeSBPayClient, fakePaymentClient, NullLogger<DepositTimeoutCheckHandler>.Instance);

        var formId = Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var request = new ScheduleRequest
        {
            FormId = formId,
            Subject = "tgs.payment.ganyopay.deposit.schedule.reject",
            ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var applyInfo = new GanyoPayDepositApplyInfo(
            new GanyoPayDepositSettings(
                "taiwan",
                "e274026fbe9e6869fbf5e0350d2e42cd",
                formId,
                "4ucmqplxb029",
                null,
                "nagad"),
            new GanyoPayDepositResponse(
                25000m,
                formId,
                new Uri("https://mobile.yoyo148148.com/deposit?orderNo=1729007289152569344"),
                "a2da47e3-04cc-4451-b600-279cc3a7bd8d"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 25000m,
                BrandId = brandId,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
            });

        _ = mockHttp
            .Expect(HttpMethod.Post, "https://www.enier13.com/api/pay_in/query")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            })
            .WithContent(await new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer_id", "taiwan" },
                { "reference_number","a2da47e3-04cc-4451-b600-279cc3a7bd8d" },
                { "token", "wKbGd8vV5eWZS2z+DbXyhRargJRXxZwg0H5qUfkoHaA=" }
            }).ReadAsStringAsync().ConfigureAwait(false))
            .Respond(HttpStatusCode.OK, new StringContent(
                JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        amount = "25000.00",
                        reference_number = "a2da47e3-04cc-4451-b600-279cc3a7bd8d",
                        fee = "1125.00",
                        custom_data = "4ucmqplxb029",
                        result = "waiting",
                        token = "5H2TSJRY8s16hF74FsfDN4LegeRjr6V3iX3UPIF9WuQ=",
                    },
                    ret_code = "SUCCESS"
                })));

        // Act
        await sut.HandleAsync(request.ToByteArray());

        // Assert
        _ = fakePaymentClient.Received(1)
        .ScheduleAsync(
                Arg.Is("tgs.payment.ganyopay.deposit.schedule.reject"),
                Arg.Is(request.FormId.ToGuid()),
                Arg.Any<DateTime>());
    }
}
