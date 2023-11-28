using System.Reflection;
using Tgs.Payment.GanyoPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGanyoPay(
       this IServiceCollection services,
       Action<GanyoPayOptions> configureOptions,
       Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<GanyoPayClient>());

        return services
            .AddOptions<GanyoPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<GanyoPayHasher>();
    }
}
