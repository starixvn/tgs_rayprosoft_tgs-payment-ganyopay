using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.GanyoPay;

namespace GanyoPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 查詢餘額()
    {
        var key = "106e16b5f7b40869aa7f98d7b7d6a7f3";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
        };
        var expected = "SFFTDkwCLkxq5mjuOyEpqpMSbjuvHvxbPlhnKzaqZoY=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 查詢存款單()
    {
        var key = "e274026fbe9e6869fbf5e0350d2e42cd";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
            ["reference_number"] = "a2da47e3-04cc-4451-b600-279cc3a7bd8d",
        };

        var expected = "wKbGd8vV5eWZS2z+DbXyhRargJRXxZwg0H5qUfkoHaA=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款()
    {
        var key = "e274026fbe9e6869fbf5e0350d2e42cd";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "25000",
            ["currency"] = "BDT",
            ["reference_number"] = "a2da47e3-04cc-4451-b600-279cc3a7bd8d",
            ["custom_data"] = "4ucmqplxb029",
            ["backend_url"] = "https://dev-payment.rpgp.cc/ganyopay/deposit/callback",
            ["channel"] = "nagad",
            ["version"] = "1.0.0",
        };

        var expected = "sCeo2EYznl2grPOoWQo2WncpnCnNMihFC6w/2wgrgrw=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款Callback()
    {
        var key = "e274026fbe9e6869fbf5e0350d2e42cd";
        var dict = new Dictionary<string, string?>
        {
            ["result"] = "success",
            ["reference_number"] = "ab5c2f3e-103c-4645-be42-1e31f803adac",
            ["fee"] = "1125.00",
            ["customer_id"] = "taiwan",
            ["custom_data"] = "4ucmqplxb029",
            ["amount"] = "25000.00",
            ["token"] = "2Kbukul4zw3/8BMxGV5VDzYjzaFIJuSZ8gi7KzHPWRQ=",
        };

        var expected = "2Kbukul4zw3/8BMxGV5VDzYjzaFIJuSZ8gi7KzHPWRQ=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款()
    {
        var key = "106e16b5f7b40869aa7f98d7b7d6a7f3";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "1000",
            ["reference_number"] = "4ebdabc9-ce7b-4639-88f9-56eadfb7b999",
            ["backend_url"] = "https://dev-payment.rpgp.cc/ganyopay/withdraw/callback",
            ["account_number"] = "testAccountNumber",
            ["account_name"] = "testAccountName",
            ["bank_name"] = "bkash"
        };

        var expected = "8Wgzy7QF+Ee6Pxr4Xx9rNWdc0eJUv0GZUDNtYpDOawY=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款Callback成功()
    {
        var key = "106e16b5f7b40869aa7f98d7b7d6a7f3";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "1000.00",
            ["reference_number"] = "211f4bda-86aa-4261-9099-83c1b2303ac7",
            ["fee"] = "25.00",
            ["custom_data"] = "",
            ["result"] = "success",            
            ["token"] = "3ZUR+0vMY2e7O+21DNpbCx7fBHAHhrpGKmUpzX4J/co=",
        };

        var expected = "3ZUR+0vMY2e7O+21DNpbCx7fBHAHhrpGKmUpzX4J/co=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款Callback失敗()
    {
        var key = "106e16b5f7b40869aa7f98d7b7d6a7f3";
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = "taiwan",
            ["amount"] = "1000.00",
            ["reference_number"] = "8d681863-fe71-4f41-aae2-90748acc6d37",
            ["fee"] = "25.00",
            ["custom_data"] = "",
            ["result"] = "fail",
            ["token"] = "WZUhAbtMQtEY4rFp04994dvis95dM8z650vGam2+VLo=",
        };

        var expected = "WZUhAbtMQtEY4rFp04994dvis95dM8z650vGam2+VLo=";

        var sut = new GanyoPayHasher(NullLogger<GanyoPayHasher>.Instance);

        var actual = sut.ComputeHash(key, dict);

        Assert.Equal(expected, actual);
    }
}
