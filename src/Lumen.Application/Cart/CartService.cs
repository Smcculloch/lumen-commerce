using Lumen.Application.Cart.Dtos;
using Lumen.Application.Products;
using Lumen.Domain.Cart;
using Lumen.Domain.Enums;
using Lumen.Shared.Extensions;

namespace Lumen.Application.Cart;

public sealed class CartService : ICartService
{
    private readonly ICartRepository _repository;
    private readonly IProductService _productService;
    private readonly ICartContextProvider _contextProvider;

    public CartService(
        ICartRepository repository,
        IProductService productService,
        ICartContextProvider contextProvider)
    {
        _repository = repository;
        _productService = productService;
        _contextProvider = contextProvider;
    }

    public async Task<CartDto> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(cancellationToken);
        return Map(cart);
    }

    public async Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var (sku, name, price) = await ResolveProductLineAsync(request, cancellationToken);
        var cart = await GetOrCreateCartAsync(cancellationToken);

        var existing = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.ProductVariantId == request.ProductVariantId);

        if (existing is not null)
        {
            existing.SetQuantity(existing.Quantity + request.Quantity);
            await _repository.UpdateAsync(cart, cancellationToken);
        }
        else
        {
            var line = CartItem.Create(
                cart.Id,
                request.ProductId,
                request.ProductVariantId,
                sku,
                name,
                request.Quantity,
                price);

            cart.Items.Add(line);
            cart.Touch();
            await _repository.UpdateAsync(cart, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(cart);
    }

    public async Task<CartDto> UpdateQuantityAsync(Guid lineItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(cancellationToken);
        var line = cart.Items.FirstOrDefault(i => i.Id == lineItemId)
            ?? throw new InvalidOperationException("Cart line was not found.");

        line.SetQuantity(quantity);
        cart.Touch();
        await _repository.UpdateAsync(cart, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid lineItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(cancellationToken);
        var line = cart.Items.FirstOrDefault(i => i.Id == lineItemId);
        if (line is not null)
        {
            cart.Items.Remove(line);
            cart.Touch();
            await _repository.UpdateAsync(cart, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Map(cart);
    }

    public async Task ClearCartAsync(CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(cancellationToken);
        if (cart.Items.Count == 0)
        {
            return;
        }

        cart.Items.Clear();
        cart.Touch();
        await _repository.UpdateAsync(cart, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeAnonymousCartOnLoginAsync(
        string sessionKey,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var anonymousCart = await _repository.GetBySessionKeyAsync(sessionKey, cancellationToken);
        if (anonymousCart is null || anonymousCart.Items.Count == 0)
        {
            return;
        }

        var customerCart = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (customerCart is null)
        {
            anonymousCart.AttachToCustomer(customerId);
            await _repository.UpdateAsync(anonymousCart, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return;
        }

        foreach (var line in anonymousCart.Items)
        {
            var existing = customerCart.Items.FirstOrDefault(i =>
                i.ProductId == line.ProductId && i.ProductVariantId == line.ProductVariantId);

            if (existing is not null)
            {
                existing.SetQuantity(existing.Quantity + line.Quantity);
            }
            else
            {
                var merged = CartItem.Create(
                    customerCart.Id,
                    line.ProductId,
                    line.ProductVariantId,
                    line.Sku,
                    line.ProductName,
                    line.Quantity,
                    line.UnitPrice);
                customerCart.Items.Add(merged);
            }
        }

        customerCart.Touch();
        await _repository.UpdateAsync(customerCart, cancellationToken);
        await _repository.DeleteAsync(anonymousCart, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ShoppingCart> GetOrCreateCartAsync(CancellationToken cancellationToken)
    {
        var context = await _contextProvider.GetCurrentAsync(cancellationToken);

        if (context.CustomerId is Guid customerId)
        {
            var customerCart = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);
            if (customerCart is not null)
            {
                return customerCart;
            }

            var newCustomerCart = ShoppingCart.CreateForCustomer(customerId);
            await _repository.AddAsync(newCustomerCart, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return newCustomerCart;
        }

        if (string.IsNullOrWhiteSpace(context.SessionKey))
        {
            throw new InvalidOperationException("Cart session is not available.");
        }

        var sessionCart = await _repository.GetBySessionKeyAsync(context.SessionKey, cancellationToken);
        if (sessionCart is not null)
        {
            return sessionCart;
        }

        var newSessionCart = ShoppingCart.CreateForSession(context.SessionKey);
        await _repository.AddAsync(newSessionCart, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return newSessionCart;
    }

    private async Task<(string Sku, string Name, decimal Price)> ResolveProductLineAsync(
        AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product was not found.");

        if (product.Status != ProductStatus.Published)
        {
            throw new InvalidOperationException("Only published products can be added to the cart.");
        }

        if (request.ProductVariantId is Guid variantId)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
                ?? throw new InvalidOperationException("Product variant was not found.");

            var variantPrice = variant.Properties.GetDecimal("price") ?? product.Properties.GetDecimal("price")
                ?? throw new InvalidOperationException("Product variant has no price.");

            return (variant.Sku, variant.Name, variantPrice);
        }

        var price = product.Properties.GetDecimal("price")
            ?? throw new InvalidOperationException("Product has no price.");

        return (product.Sku, product.Name, price);
    }

    private static CartDto Map(ShoppingCart cart) =>
        new(
            cart.Id,
            cart.CustomerId,
            cart.SessionKey,
            cart.Items
                .OrderBy(i => i.ProductName)
                .Select(i => new CartItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductVariantId,
                    i.Sku,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal))
                .ToList(),
            cart.ItemCount,
            cart.Subtotal);
}