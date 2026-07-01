using Lumen.Application.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Infrastructure.Payments;

public static class PaymentServiceCollectionExtensions
{
    public static IServiceCollection AddLumenPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentOptions>(configuration.GetSection(PaymentOptions.SectionName));
        services.AddSingleton<IPaymentProvider, DummyPaymentProvider>();
        services.AddSingleton<IPaymentProviderResolver, PaymentProviderResolver>();

        return services;
    }
}