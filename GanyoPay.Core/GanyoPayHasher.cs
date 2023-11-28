using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.GanyoPay;

public class GanyoPayHasher
{
    private readonly ILogger<GanyoPayHasher> _logger;

    public GanyoPayHasher(ILogger<GanyoPayHasher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string ComputeHash(string privateKey, IEnumerable<KeyValuePair<string, string?>> dict)
    {
        var content = string.Join(
            "",
            dict.Where(kvp => string.IsNullOrWhiteSpace(kvp.Value) is false && kvp.Key is not "token")
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Value!.ToLower()}"));

        var fullText = $"{content}{privateKey}";

        _logger.LogTrace("FullText: {fullText}", fullText);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

        return Convert.ToBase64String(hash);
    }
}
