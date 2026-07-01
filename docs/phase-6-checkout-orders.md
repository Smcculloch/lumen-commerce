# Phase 6 — Checkout + Order Creation (with CMS Integration)

Phase 6 adds a single-page checkout experience linked to CMS content, guest checkout, address capture, order creation from the cart, and basic order management in the backoffice. Payment processing is intentionally out of scope.

## Delivered capabilities

### Domain & services

- **Order** — customer/guest info, shipping + billing addresses, order notes, status, subtotal, order number
- **OrderLineItem** — snapshotted product data (SKU, name, quantity, unit price) from cart lines
- **OrderAddress** — embedded value object persisted as columns on `Orders`
- **OrderStatus** — `Pending`, `Processing`, `Shipped`, `Completed`, `Cancelled`
- **IOrderService** — place order from cart, list/filter, get by id/number, update status
- **IOrderRepository** — EF persistence with client-side sort (SQLite `DateTimeOffset` workaround)

### CMS integration

- **`checkout-page` template** (`CheckoutPageTemplate`) — title, slug, introduction, body, terms & policies
- Seeded published CMS page at `/checkout` (`ContentSeedData`)
- `CheckoutController` loads CMS content via `IContentService.GetContentBySlugAsync("checkout")` and renders editorial content above the functional checkout form (same pattern as `shop-page` + `Shop.cshtml`)

### Storefront (`Lumen.Storefront`)

| Route | Purpose |
|-------|---------|
| `/checkout` | CMS header/terms + cart summary + checkout form |
| `/checkout` (POST) | Validate form, create order, clear cart, redirect to confirmation |
| `/checkout/confirmation/{orderNumber}` | Order thank-you page with line items and totals |

**Checkout form includes:**

- Contact (name, email) — guest checkout supported; logged-in users prefilled
- Shipping address
- Billing address (optional “same as shipping”)
- Order notes (optional)
- Cart summary sidebar

**Guest checkout:** no account required; guest name, email, and addresses stored on the order. `CustomerId` is set when the shopper is authenticated.

### Backoffice (`Lumen.Backoffice`)

| Route | Purpose |
|-------|---------|
| `/orders` | Order list with search and status filter |
| `/orders/{id}` | Order detail — customer, addresses, line items, status update |

**Nav:** Orders appears after Customers in the sidebar.

**Components:**

- `Components/Orders/OrderStatusBadge.razor`

### Database

- Shared SQLite: `LumenCommerce/data/lumencommerce.db` via `DatabasePath.Resolve`
- Migration: `Phase6_Orders` — `Orders`, `OrderLineItems` tables
- Seed: `/checkout` CMS page added to `ContentSeedData` (upgrade-safe)

### Bug fixes included in this phase

- **Cart price resolution** — `CartService` and `ProductDetailViewModel` now use `PropertyBagExtensions.GetDecimal("price")` so prices stored as JSON `JsonElement` values are read correctly
- **Cart line persistence** — new cart items inserted via `_dbContext.CartItems.AddAsync()` to avoid EF concurrency errors on first add

## Architecture notes

- Commerce route `/checkout` is handled by `CheckoutController` (explicit route wins over `ContentController` catch-all)
- CMS content is loaded by slug; the functional form lives in `Views/Checkout/Index.cshtml`
- `IOrderService` is registered in `AddLumenApplication()`; order placement orchestrates cart read → order create → `ICartService.ClearCartAsync()`
- Order numbers: `LMN-{yyyyMMdd}-{sequence}` (e.g. `LMN-20260630-00001`)

## Verification checklist

- [x] Single-page checkout exists at `/checkout` and is linked to a CMS content page
- [x] CMS content (intro, body, terms) can be displayed on the checkout page
- [x] Guest checkout works without requiring login
- [x] Shipping and billing addresses are captured and saved with the order
- [x] Order + OrderLineItems are created successfully from the cart
- [x] Cart is cleared after successful checkout
- [x] Order confirmation page is shown with order details
- [x] Backoffice has an Orders section with list, detail view, and status update capability

## Run locally

```bash
dotnet ef database update --project src/Lumen.Infrastructure --startup-project src/Lumen.Backoffice
dotnet run --project src/Lumen.Storefront --urls http://localhost:5267
dotnet run --project src/Lumen.Backoffice --urls http://localhost:5258
```

**Try:** browse `/shop` → add product to cart → `/checkout` → place order as guest → view confirmation → open **Orders** in backoffice to review and update status.

**CMS:** edit the Checkout page in **Content** to change introduction, body, or terms — publish to see changes on `/checkout`.

## Out of scope (future phases)

- Payment processing
- Tax and shipping calculation
- Order confirmation emails
- Inventory decrement
- Saved customer address book