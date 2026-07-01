using Lumen.Application.Cart;
using Lumen.Application.Jobs;
using Microsoft.Extensions.Options;

namespace Lumen.Infrastructure.Jobs.Handlers;

public sealed class CartCleanupJobHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly JobOptions _options;

    public CartCleanupJobHandler(ICartRepository cartRepository, IOptions<JobOptions> options)
    {
        _cartRepository = cartRepository;
        _options = options.Value;
    }

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken)
    {
        var olderThan = DateTimeOffset.UtcNow.AddDays(-_options.CartCleanup.MaxAgeDays);
        var deleted = await _cartRepository.DeleteStaleCartsAsync(olderThan, cancellationToken);
        return $"Deleted {deleted} stale empty cart(s) older than {_options.CartCleanup.MaxAgeDays} day(s).";
    }
}