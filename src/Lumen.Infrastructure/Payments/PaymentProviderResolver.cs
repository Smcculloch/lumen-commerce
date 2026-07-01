using Lumen.Application.Payments;
using Microsoft.Extensions.Options;

namespace Lumen.Infrastructure.Payments;

public sealed class PaymentProviderResolver : IPaymentProviderResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentProvider> _providersByName;
    private readonly PaymentOptions _options;

    public PaymentProviderResolver(IEnumerable<IPaymentProvider> providers, IOptions<PaymentOptions> options)
    {
        _providersByName = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
    }

    public IPaymentProvider ResolveActiveProvider()
    {
        var name = string.IsNullOrWhiteSpace(_options.Provider)
            ? DummyPaymentProvider.ProviderName
            : _options.Provider.Trim();

        if (!_providersByName.TryGetValue(name, out var provider))
        {
            throw new InvalidOperationException(
                $"Payment provider '{name}' is not registered. Available: {string.Join(", ", _providersByName.Keys)}.");
        }

        return provider;
    }

    public IReadOnlyList<IPaymentProvider> GetAllProviders() =>
        _providersByName.Values.ToList();
}