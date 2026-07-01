namespace Lumen.Application.Jobs;

public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    public bool Enabled { get; set; } = true;

    public AbandonedCartJobOptions AbandonedCart { get; set; } = new();

    public CartCleanupJobOptions CartCleanup { get; set; } = new();

    public OrderStatusJobOptions OrderStatus { get; set; } = new();

    public InventorySyncJobOptions InventorySync { get; set; } = new();
}

public sealed class AbandonedCartJobOptions
{
    public int InactiveHours { get; set; } = 24;

    public string CronExpression { get; set; } = "0 0 * * * ?";
}

public sealed class CartCleanupJobOptions
{
    public int MaxAgeDays { get; set; } = 30;

    public string CronExpression { get; set; } = "0 30 2 * * ?";
}

public sealed class OrderStatusJobOptions
{
    public int PendingHours { get; set; } = 48;

    public string CronExpression { get; set; } = "0 0 6 * * ?";
}

public sealed class InventorySyncJobOptions
{
    public string CronExpression { get; set; } = "0 0 3 * * ?";
}