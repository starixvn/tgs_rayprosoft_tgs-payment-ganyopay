using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.Clients;
using Tgs.Payment.GanyoPay;
using Tgs.Payment.GanyoPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigQueueOptions((opt, _) => opt.Url = "nats://localhost:4222"))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var customerId = "taiwan";

//Deposit
//var privateKey = "e274026fbe9e6869fbf5e0350d2e42cd";

//var depositSettings = new GanyoPayDepositSettings(customerId, privateKey, Guid.NewGuid(), "4ucmqplxb029", null, "nagad");

//var depositReturn = await paymentClient.DepositAsync(
//    "tgs.payment.ganyopay.deposit",
//    25000m,
//    JsonSerializer.Serialize(depositSettings, GanyoPayUtility.SerializerOptions)).ConfigureAwait(false);

//logger.LogInformation("DepositReturn: {depositReturn}", depositReturn);


//Withdraw
//var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

//var withdrawSettings = new GanyoPayWithdrawSettings(customerId, privateKey, Guid.NewGuid(), "testAccountNumber", "testAccountName", "bkash",null);

//var withdrawReturn = await paymentClient.WithdrawAsync(
//    "tgs.payment.ganyopay.withdraw",
//    1000m,
//    JsonSerializer.Serialize(withdrawSettings, GanyoPayUtility.SerializerOptions)).ConfigureAwait(false);

//logger.LogInformation("WithdrawReturn: {withdrawReturn}", withdrawReturn);



//查詢存款單
//var httpClient = new HttpClient();
//httpClient.BaseAddress = new Uri("https://www.enier13.com");
//var client = new GanyoPayClient(httpClient, new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance), NullLogger<GanyoPayClient>.Instance);

//var queryReturn = await client.QueryDepositFormStatusAsync(customerId, "e274026fbe9e6869fbf5e0350d2e42cd", Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d"));
//logger.LogInformation("{return}", queryReturn);


//商戶餘額查詢
var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://www.enier13.com");
var client = new GanyoPayClient(httpClient, new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance), NullLogger<GanyoPayClient>.Instance);

var queryReturn = await client.QueryBalanceAsync(customerId, "106e16b5f7b40869aa7f98d7b7d6a7f3");
logger.LogInformation("{return}", queryReturn);