using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;
using Tgs.Payment.GanyoPay.MessageQueue;
using Tgs.Payment.Protos;

namespace GanyoPay.Core.UnitTests;

public class DepositTimeoutCheckHandlerTests
{
    [Fact]
    public async Task 申請三十一分鐘的單子會自動拒絕()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new DepositTimeoutCheckHandler(fakePaymentServiceClient, fakePaymentClient, NullLogger<DepositTimeoutCheckHandler>.Instance);

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

        // Act
        await sut.HandleAsync(request.ToByteArray());

        // Assert
        _ = fakePaymentClient
            .Received(1)
            .RejectDepositAsync(Arg.Is(formId), Arg.Is(brandId));
    }
}
