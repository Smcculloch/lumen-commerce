using Lumen.Application.Products.Dtos;
using Lumen.Domain.Enums;

namespace Lumen.Application.Products;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> ListAsync(Guid? categoryId, ProductStatus? status, string? search, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> PublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> UnpublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariantDto>> GetVariantsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductVariantDto> CreateVariantAsync(Guid productId, CreateVariantRequest request, CancellationToken cancellationToken = default);
    Task<ProductVariantDto> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, CancellationToken cancellationToken = default);
    Task DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);
}