using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.GanyoPay;

public class GanyoPayClient
{
    private readonly HttpClient _httpClient;
    private readonly GanyoPayHasher _hasher;
    private readonly ILogger<GanyoPayClient> _logger;

    public GanyoPayClient(
        HttpClient httpClient,
        GanyoPayHasher hasher,
        ILogger<GanyoPayClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<GanyoPayClientDepositResponse> DepositAsync(string customerId, string privateKey, DepositOrder order, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = customerId,
            ["amount"] = order.Amount.ToString(),
            ["currency"] = "BDT",
            ["reference_number"] = order.FormId.ToString(),
            ["custom_data"] = order.CustomData,
            ["backend_url"] = order.CallbackUrl.AbsoluteUri,
            ["channel"] = order.Channel,
            ["version"] = "1.0.0",
        };

        if (order.PhoneNumber != null)
        {
            dict["phone_number"] = order.PhoneNumber;
        }

        var token = _hasher.ComputeHash(privateKey, dict);
        dict["token"] = token;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/pay_in")
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("GanyoPayClient Deposit Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Deposit response can't read.");

        if (node["ret_code"]!.GetValue<string>() != "SUCCESS")
            throw new ApiFailException(node["err_msg"]!.GetValue<string>());

        var data = node["data"]!;

        return new GanyoPayClientDepositResponse(
            Amount: decimal.Parse(data["amount"]!.GetValue<string>()),
            FormId: Guid.Parse(data["reference_number"]!.GetValue<string>()),
            ThirdPartyOrderNo: data["reference_number"]!.GetValue<string>(),
            RedirectUrl: new Uri(data["payment_url"]!.GetValue<string>()));
    }

    public async ValueTask<string> QueryDepositFormStatusAsync(string customerId, string privateKey, Guid formId, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = customerId,
            ["reference_number"] = formId.ToString(),
        };

        var token = _hasher.ComputeHash(privateKey, dict);
        dict["token"] = token;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/pay_in/query")
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("GanyoPayClient Query Deposit Status Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit Status response can't read.");

        var data = node["data"]!;

        return data["result"]!.GetValue<string>();
    }

    public async ValueTask<GanyoPayClientWithdrawResponse> WithdrawAsync(string customerId, string privateKey, WithdrawOrder order, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = customerId,
            ["amount"] = order.Amount.ToString(),            
            ["reference_number"] = order.FormId.ToString(),
            ["backend_url"] = order.CallbackUrl.AbsoluteUri,
            ["account_number"] = order.AccountNumber,
            ["account_name"] = order.AccountName,
            ["bank_name"] = order.BankName,
        };

        if (order.Channel != null)
        {
            dict["channel"] = order.Channel;
        }

        var token = _hasher.ComputeHash(privateKey, dict);
        dict["token"] = token;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/pay_out")
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("GanyoPayClient Withdraw Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Withdraw response can't read.");

        if (node["ret_code"]!.GetValue<string>() != "SUCCESS")
            throw new ApiFailException(node["err_msg"]!.GetValue<string>());

        var data = node["data"]!;

        return new GanyoPayClientWithdrawResponse(
            Amount: decimal.Parse(data["amount"]!.GetValue<string>()),
            FormId: Guid.Parse(data["reference_number"]!.GetValue<string>()),
            ThirdPartyOrderNo: data["reference_number"]!.GetValue<string>());
    }

    public async ValueTask<GanyoPayClientQueryBalanceResponse> QueryBalanceAsync(string customerId, string privateKey, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["customer_id"] = customerId,
        };

        var token = _hasher.ComputeHash(privateKey, dict);
        dict["token"] = token;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/balance/query")
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("GanyoPayClient Query Merchant Balance Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Merchant Balance response can't read.");

        if (node["ret_code"]!.GetValue<string>() != "SUCCESS")
            throw new ApiFailException(node["err_msg"]!.GetValue<string>());

        var data = node["data"]!;
        var availableBalance = data["available"]!.GetValue<string>();
        var frozenBalance = data["frozen"]!.GetValue<string>();

        return new GanyoPayClientQueryBalanceResponse(
            decimal.Parse(availableBalance),
            decimal.Parse(frozenBalance));
    }
}
