namespace Lumen.Infrastructure.Jobs.Handlers;

/// <summary>
/// Stub inventory sync job for future ERP/PIM integration.
/// </summary>
public sealed class InventorySyncJobHandler
{
    public Task<string> ExecuteAsync(CancellationToken cancellationToken) =>
        Task.FromResult("Inventory sync stub completed (no external system configured).");
}