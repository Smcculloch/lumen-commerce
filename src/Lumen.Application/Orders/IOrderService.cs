using Lumen.Application.Cart.Dtos;
using Lumen.Application.Orders.Dtos;
using Lumen.Application.Payments.Dtos;
using Lumen.Domain.Enums;

namespace Lumen.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderFromCartAsync(CartDto cart, PlaceOrderRequest request, CancellationToken cancellationToken = default);
    Task<CheckoutResult> CheckoutWithPaymentAsync(
        CartDto cart,
        PlaceOrderRequest placeRequest,
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task<OrderDto> CapturePaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> RefundPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default);
}