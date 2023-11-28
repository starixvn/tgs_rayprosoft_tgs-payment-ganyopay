using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.GanyoPay.IntegrationTests;

public class WithdrawTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public WithdrawTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 代付回調_訂單狀態成功()
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

        var formId = Guid.Parse("211f4bda-86aa-4261-9099-83c1b2303ac7");
        var brandId = Guid.NewGuid();
        var customerId = "taiwan";
        var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

        var settings = new GanyoPayWithdrawSettings(
            customerId,
            privateKey,
            formId,
            "testAccountNumber",
            "testAccountName",
            "bkash",
            null);
        var withdrawResponse = new GanyoPayWithdrawResponse(formId);

        var applyInfo = new GanyoPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
            });

        var dict = new Dictionary<string, string>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "1000.00",
            ["reference_number"] = "211f4bda-86aa-4261-9099-83c1b2303ac7",
            ["fee"] = "25.00",
            ["custom_data"] = "",
            ["result"] = "success",
            ["token"] = "3ZUR+0vMY2e7O+21DNpbCx7fBHAHhrpGKmUpzX4J/co=",
        };

        var content = new FormUrlEncodedContent(dict);

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態失敗()
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

        var formId = Guid.Parse("8d681863-fe71-4f41-aae2-90748acc6d37");
        var brandId = Guid.NewGuid();
        var customerId = "taiwan";
        var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

        var settings = new GanyoPayWithdrawSettings(
            customerId,
            privateKey,
            formId,
            "testAccountNumber",
            "testAccountName",
            "bkash",
            null);
        var withdrawResponse = new GanyoPayWithdrawResponse(formId);

        var applyInfo = new GanyoPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, GanyoPayUtility.SerializerOptions)
            });

        var dict = new Dictionary<string, string>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "1000.00",
            ["reference_number"] = "8d681863-fe71-4f41-aae2-90748acc6d37",
            ["fee"] = "25.00",
            ["custom_data"] = "",
            ["result"] = "fail",
            ["token"] = "WZUhAbtMQtEY4rFp04994dvis95dM8z650vGam2+VLo=",
        };

        var content = new FormUrlEncodedContent(dict);

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}
