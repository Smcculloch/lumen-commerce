namespace Lumen.Domain.Enums;

public enum PaymentStatus
{
    None = 0,
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyRefunded = 6
}