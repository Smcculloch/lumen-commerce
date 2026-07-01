namespace Lumen.Application.Payments;

public sealed class PaymentOptions
{
    public const string SectionName = "Payments";

    public string Provider { get; set; } = "dummy";

    public string Currency { get; set; } = "SEK";

    public DummyPaymentOptions Dummy { get; set; } = new();
}

public sealed class DummyPaymentOptions
{
    public bool SimulateFailure { get; set; }

    public string FailureMessage { get; set; } = "Payment declined (simulated).";
}