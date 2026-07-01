using Lumen.Application.Cart;
using Lumen.Application.Customers;
using Lumen.Application.Jobs;
using Lumen.Domain.Jobs;
using Microsoft.Extensions.Options;

namespace Lumen.Infrastructure.Jobs.Handlers;

public sealed class AbandonedCartJobHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly JobOptions _options;

    public AbandonedCartJobHandler(
        ICartRepository cartRepository,
        ICustomerRepository customerRepository,
        INotificationLogRepository notificationLogRepository,
        IOptions<JobOptions> options)
    {
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
        _notificationLogRepository = notificationLogRepository;
        _options = options.Value;
    }

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken)
    {
        var inactiveSince = DateTimeOffset.UtcNow.AddHours(-_options.AbandonedCart.InactiveHours);
        var carts = await _cartRepository.ListInactiveWithItemsAsync(inactiveSince, cancellationToken);

        var sent = 0;
        foreach (var cart in carts)
        {
            string? recipient = null;
            if (cart.CustomerId is Guid customerId)
            {
                var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
                recipient = customer?.Email;
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                continue;
            }

            var itemCount = cart.Items.Sum(i => i.Quantity);
            var subject = "You left items in your cart";
            var body =
                $"Reminder: your cart still has {itemCount} item(s). " +
                $"Return to the store to complete your purchase.";

            await _notificationLogRepository.AddAsync(
                NotificationLog.Create(
                    "AbandonedCart",
                    recipient,
                    subject,
                    body,
                    cart.Id),
                cancellationToken);

            sent++;
        }

        if (sent > 0)
        {
            await _notificationLogRepository.SaveChangesAsync(cancellationToken);
        }

        return $"Processed {carts.Count} inactive cart(s); queued {sent} abandoned-cart notification(s).";
    }
}