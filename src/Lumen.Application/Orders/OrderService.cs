using Lumen.Application.Cart.Dtos;
using Lumen.Application.Orders.Dtos;
using Lumen.Domain.Enums;
using Lumen.Domain.Orders;

namespace Lumen.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> PlaceOrderFromCartAsync(
        CartDto cart,
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cannot place an order from an empty cart.");
        }

        var shipping = MapAddress(request.ShippingAddress);
        var billing = MapAddress(request.BillingAddress);

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var lines = cart.Items
            .Select(i => new Order.OrderLineInput(
                i.ProductId,
                i.ProductVariantId,
                i.Sku,
                i.ProductName,
                i.Quantity,
                i.UnitPrice))
            .ToList();

        var order = Order.Create(
            orderNumber,
            request.CustomerId,
            request.CustomerName,
            request.Email,
            shipping,
            billing,
            request.OrderNotes,
            lines);

        await _repository.AddAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(order);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByOrderNumberAsync(orderNumber, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<IReadOnlyList<OrderDto>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken = default)
    {
        var orders = await _repository.ListAsync(filter, cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        order.SetStatus(status);
        await _repository.UpdateAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(order);
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _repository.CountAsync(cancellationToken);
        return $"LMN-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D5}";
    }

    private static OrderAddress MapAddress(OrderAddressDto dto) =>
        OrderAddress.Create(
            dto.Name,
            dto.Line1,
            dto.Line2,
            dto.City,
            dto.Region,
            dto.PostalCode,
            dto.Country);

    private static OrderDto Map(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.CustomerName,
            order.Email,
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            order.OrderNotes,
            order.Status,
            order.Subtotal,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items
                .OrderBy(i => i.ProductName)
                .Select(i => new OrderLineItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductVariantId,
                    i.Sku,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal))
                .ToList());

    private static OrderAddressDto MapAddress(OrderAddress address) =>
        new(
            address.Name,
            address.Line1,
            address.Line2,
            address.City,
            address.Region,
            address.PostalCode,
            address.Country);
}