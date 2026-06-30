# Phase 5 — Customer + Cart + Foundational Storefront

Phase 5 adds a lightweight customer layer, full shopping cart functionality, and storefront commerce UX.

## Delivered capabilities

### Customer layer

- `Customer` domain entity linked to Identity `ApplicationUser`
- `ICustomerService` / `ICustomerRepository`
- Auto-created on storefront registration
- Backoffice **Customers** section (`/customers`, `/customers/{id}`)

### Shopping cart

- `ShoppingCart` + `CartItem` persisted in shared database
- **Anonymous carts** — keyed by ASP.NET session (`LumenCartSessionKey`)
- **Authenticated carts** — linked to `CustomerId`
- **Cart merge on login/register** — anonymous items merge into customer cart
- `ICartService` — add, update quantity, remove, clear, get summary
- Prices resolved from published PIM product/variant properties

### Storefront

| Route | Purpose |
|-------|---------|
| `/products` | Published product catalog |
| `/products/{sku}` | Product detail with add-to-cart |
| `/cart` | Cart review and management |
| `/checkout` | Checkout placeholder (Phase 6) |
| `/account/login` | Login with cart merge |
| `/account/register` | Registration + customer creation |

**Navigation:** Products link, cart icon with item count badge, Login/Register.

### Identity

- ASP.NET Core Identity with `ApplicationUser`
- Cookie authentication on storefront
- Shared `AppDbContext` (Identity + commerce tables)

## Verification checklist

- [x] Lightweight `Customer` entity and backoffice Customers section
- [x] Anonymous users can add items to cart
- [x] Logged-in users have persistent carts
- [x] Cart merges on login with existing anonymous cart
- [x] Cart add/update/remove/clear works
- [x] Product detail pages render seeded PIM data; `/products` lists catalog
- [x] Cart icon shows item count in header
- [x] Checkout placeholder at `/checkout`
- [x] Login/Register accessible from storefront

## Run locally

```bash
dotnet ef database update --project src/Lumen.Infrastructure --startup-project src/Lumen.Backoffice
dotnet run --project src/Lumen.Storefront
```

Try: browse `/products` → add to cart → register → verify cart persists after login.