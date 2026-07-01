namespace Lumen.Application.Payments;

public interface IPaymentProviderResolver
{
    IPaymentProvider ResolveActiveProvider();
    IReadOnlyList<IPaymentProvider> GetAllProviders();
}