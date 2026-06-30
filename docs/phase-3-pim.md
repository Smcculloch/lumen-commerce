# Phase 3 — PIM Module

Phase 3 adds Product Information Management to Lumen Commerce, reusing the hybrid template system from Phase 2.

## Delivered capabilities

### Domain & services

- **Product** — linked to a product template; supports `CategoryId`, `SortOrder`, `ProductStatus`, and publish/unpublish lifecycle
- **ProductVariant** — child SKUs with optional property overrides
- **Category** — hierarchical catalog organization (`ParentId`, materialized path, `FullPath`)
- **IProductService** — CRUD, list/filter, publish/unpublish, variant management
- **ICategoryService** — category tree CRUD (mirrors content path patterns)

### Backoffice (`Lumen.Backoffice`)

| Route | Purpose |
|-------|---------|
| `/products` | Category tree + product list with search, status filter, publish/delete |
| `/products/new` | Template picker → dynamic product editor |
| `/products/edit/{id}` | Edit product, template fields, variant list |

**Nav:** Products appears after Content in the sidebar.

**Components:**

- `Components/Products/CategoryTreeHelper.cs` — tree flattening/search (content pattern)
- `Components/Products/ProductStatusBadge.razor` — status badge
- Reuses `TemplatePropertyForm`, `AlertBanner`, `PropertyValueConverter` from Content

### Storefront (`Lumen.Storefront`)

| Route | Purpose |
|-------|---------|
| `/products/{sku}` | Published product detail page |

### Database

- Shared SQLite: `LumenCommerce/data/lumencommerce.db` via `DatabasePath.Resolve`
- Migration: `Phase3_PimModule` — Categories, ProductVariants, Product schema upgrade
- Seed: `PimSeedData` — Electronics → Laptops/Smartphones, 3 products, 1 variant (upgrade-safe)

## Verification checklist

- [x] Products and categories stored in shared `lumencommerce.db`
- [x] Backoffice `/products` shows categories and products with hierarchy/filtering
- [x] Creating/editing a product dynamically renders fields from its `ProductTemplate`
- [x] Variants can be added to a product
- [x] Seeded products and categories appear correctly
- [x] `IProductService` follows similar patterns to `IContentService`

## Run locally

```bash
dotnet run --project src/Lumen.Backoffice
dotnet run --project src/Lumen.Storefront
```

**Backoffice:** https://localhost:7xxx/products  
**Storefront examples:** `/products/LUM-LAP-15`, `/products/LUM-PHX-128`