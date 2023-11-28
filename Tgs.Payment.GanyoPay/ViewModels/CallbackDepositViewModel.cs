using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.GanyoPay.ViewModels;

public class CallbackDepositViewModel
{
    [FromForm(Name = "customer_id")]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = default!;

    [FromForm(Name = "amount")]
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = default!;

    [FromForm(Name = "reference_number")]
    [JsonPropertyName("reference_number")]
    public string ReferenceNumber { get; set; } = default!;

    [FromForm(Name = "custom_data")]
    [JsonPropertyName("custom_data")]
    public string CustomData { get; set; } = default!;

    [FromForm(Name = "fee")]
    [JsonPropertyName("fee")]
    public string Fee { get; set; } = default!;

    [FromForm(Name = "result")]
    [JsonPropertyName("result")]
    public string Result { get; set; } = default!;

    [FromForm(Name = "token")]
    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;
}
