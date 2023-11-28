using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.GanyoPay.IntegrationTests;

public class DepositTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DepositTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = GanyoPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("ab5c2f3e-103c-4645-be42-1e31f803adac");
        var brandId = Guid.NewGuid();
        var customerId = "taiwan";
        var privateKey = "e274026fbe9e6869fbf5e0350d2e42cd";
        var customData = "4ucmqplxb029";

        var dict = new Dictionary<string, string>
        {
            ["result"] = "success",
            ["reference_number"] = "ab5c2f3e-103c-4645-be42-1e31f803adac",
            ["fee"] = "1125.00",
            ["customer_id"] = "taiwan",
            ["custom_data"] = "4ucmqplxb029",
            ["amount"] = "25000.00",
            ["token"] = "2Kbukul4zw3/8BMxGV5VDzYjzaFIJuSZ8gi7KzHPWRQ=",
        };

        var content = new FormUrlEncodedContent(dict);

        var applyInfo = new GanyoPayDepositApplyInfo(
            new GanyoPayDepositSettings(
                customerId,
                privateKey,
                formId,
                customData,
                null,
                "nagad"),
             new GanyoPayDepositResponse(
                25000m,
                formId,
                new Uri("https://mobile.yoyo148148.com/deposit?orderNo=1729007289152569344"),
                formId.ToString()));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 25000m,
                ThirdPartyOrderNo = formId.ToString(),
                ApplyInfo = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);

        var paymentReturn = new GanyoPayDepositReturn()
        {
            Status = dict["result"],
            FormId = Guid.Parse(dict["reference_number"]),
            ThirdPartyOrderNo = dict["reference_number"],
            RealAmount = decimal.Parse(dict["amount"]) - decimal.Parse(dict["fee"]),
            OrderAmount = decimal.Parse(dict["amount"]),
            Sign = dict["token"],
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, GanyoPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.RealAmount),
                Arg.Any<CancellationToken>());
    }
}
