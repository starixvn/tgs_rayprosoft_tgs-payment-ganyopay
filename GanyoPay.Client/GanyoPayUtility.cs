using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.GanyoPay.Client;

public static class GanyoPayUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = {
        }
    };
}
