# Suggested Prompt for Phase 3 (PIM Module)

Copy and use this prompt to continue building Lumen Commerce:

---

You are **Grok Build**, continuing work on **Lumen Commerce** at `C:\Projects\Home\LumenCommerce`.

Phases 0–2 are complete. The hybrid template system and CMS module (`IContentService`, content tree, publishing) are in place.

## Phase 3 Goal: PIM Module

Implement the core **Product Information Management** bounded context, mirroring the CMS patterns established in Phase 2.

### Requirements

1. **Extend Product Domain**
   - Categories / catalog hierarchy (parent-child, similar to content tree)
   - `ProductStatus` (Draft, Active, Discontinued)
   - Optional `Variant` support (SKU, attributes, price override) — keep simple
   - Pricing fields validated via `standard-product` template

2. **Application Services**
   - Replace/extend `IProductService` with full `IProductCatalogService`:
     - `GetProductById`, `GetProductBySku`
     - `GetProductsByCategory`
     - `CreateProduct`, `UpdateProduct`, `ActivateProduct`, `DeactivateProduct`, `DeleteProduct`
   - Category service: `ICategoryService` with tree operations

3. **Template Integration**
   - Validate product properties against resolved `ProductTemplateDefinition`
   - Demonstrate code-defined `standard-product` plus DB template extensions

4. **Infrastructure**
   - `CategoryEntity`, extend `ProductEntity` with hierarchy/status
   - `IProductRepository` → richer `IProductCatalogRepository`
   - EF migration `Phase3_PimModule`
   - Seed sample category + products

5. **Scope Rules**
   - No cart, checkout, or storefront commerce yet
   - Minimal Backoffice page listing products (optional)
   - No CMS changes unless required for shared patterns

6. **Documentation**
   - Update `README.md` with PIM section
   - End with summary, design decisions, and Phase 4 (Commerce) prompt

Begin Phase 3 now.

---