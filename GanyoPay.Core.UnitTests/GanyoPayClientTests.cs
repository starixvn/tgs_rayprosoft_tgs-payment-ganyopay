using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.GanyoPay;

namespace GanyoPay.Core.UnitTests;

public class GanyoPayClientTests
{
    [Fact]
    public async Task 查詢商戶餘額()
    {
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var signer = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var sut = new GanyoPayClient(httpClient, signer, NullLogger<GanyoPayClient>.Instance);

        var customerId = "taiwan";
        var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

        _ = mockHttp
            .Expect(HttpMethod.Post, "https://www.enier13.com/api/balance/query")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Content-Type","application/x-www-form-urlencoded" }
            })
            .WithContent(await new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer_id", "taiwan" },
                { "token", "SFFTDkwCLkxq5mjuOyEpqpMSbjuvHvxbPlhnKzaqZoY=" }
            }).ReadAsStringAsync().ConfigureAwait(false))
            .Respond(HttpStatusCode.OK, new StringContent(
                JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        available = ".00",
                        frozen = "0",
                        alipay = "0",
                        token = "C482g4MEQSD6zcTnEgNIME3egeTb8zhPRaN8noeXPzc=",
                    },
                    ret_code = "SUCCESS"
                })));

        var actual = await sut.QueryBalanceAsync(customerId, privateKey);

        Assert.Equal(0, actual.AvailableBalance);
    }

    [Fact]
    public async Task 查詢存款單狀態()
    {
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var signer = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var sut = new GanyoPayClient(httpClient, signer, NullLogger<GanyoPayClient>.Instance);

        var customerId = "taiwan";
        var privateKey = "e274026fbe9e6869fbf5e0350d2e42cd";

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

        var actual = await sut.QueryDepositFormStatusAsync(customerId, privateKey, Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d"));

        Assert.Equal("waiting", actual);
    }

    [Fact]
    public async Task 存款()
    {
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var signer = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var sut = new GanyoPayClient(httpClient, signer, NullLogger<GanyoPayClient>.Instance);

        var customerId = "taiwan";
        var privateKey = "e274026fbe9e6869fbf5e0350d2e42cd";

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

        var formId = Guid.Parse("a2da47e3-04cc-4451-b600-279cc3a7bd8d");
        var order = new DepositOrder(
            25000m,
            formId,
            new Uri("https://dev-payment.rpgp.cc/ganyopay/deposit/callback"),
            "4ucmqplxb029",
            null,
            "nagad");

        var actual = await sut.DepositAsync(customerId, privateKey, order);

        Assert.Equal("a2da47e3-04cc-4451-b600-279cc3a7bd8d", actual.ThirdPartyOrderNo);
    }

    [Fact]
    public async Task 提款()
    {
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://www.enier13.com");

        var signer = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var sut = new GanyoPayClient(httpClient, signer, NullLogger<GanyoPayClient>.Instance);

        var customerId = "taiwan";
        var privateKey = "106e16b5f7b40869aa7f98d7b7d6a7f3";

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

        var formId = Guid.Parse("4ebdabc9-ce7b-4639-88f9-56eadfb7b999");
        var order = new WithdrawOrder(
            formId,
            1000m,
            new Uri("https://dev-payment.rpgp.cc/ganyopay/withdraw/callback"),
            "testAccountNumber",
            "testAccountName",
            "bkash",
            null);

        var actual = await sut.WithdrawAsync(customerId, privateKey, order);

        Assert.Equal("4ebdabc9-ce7b-4639-88f9-56eadfb7b999", actual.ThirdPartyOrderNo);
    }
}
