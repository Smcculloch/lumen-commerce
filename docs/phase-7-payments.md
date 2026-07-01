# Phase 7 — Payment Provider Integration

Phase 7 adds an extensible payment provider abstraction, a dummy provider for development/testing, checkout integration, and backoffice payment visibility with manual capture/refund actions.

## Delivered capabilities

### Payment abstraction

- **`IPaymentProvider`** — `ProcessPaymentAsync`, `CaptureAsync`, `RefundAsync`
- **`IPaymentProviderResolver`** — resolves the active provider from configuration
- **`PaymentOptions`** — `Payments:Provider`, `Payments:Currency`, provider-specific sections (e.g. `Payments:Dummy`)
- **`DummyPaymentProvider`** — simulates authorize/capture/refund; fails when:
  - `Payments:Dummy:SimulateFailure` is `true`
  - Checkout "Simulate payment failure" checkbox is checked
  - Customer email contains `fail@`

### Domain & persistence

- **`PaymentStatus`** — `None`, `Pending`, `Authorized`, `Captured`, `Failed`, `Refunded`, `PartiallyRefunded`
- **`Order`** extended with payment fields and methods: `ApplyPaymentOutcome`, `ApplyPaymentFailure`, `ApplyCapture`, `ApplyRefund`
- **`Orders` table** — `PaymentStatus`, `PaymentProvider`, `PaymentTransactionId`, `PaymentMessage`, `AmountCaptured`, `AmountRefunded`
- Migration: `Phase7_Payments`

### Application services

- **`IOrderService.CheckoutWithPaymentAsync`** — create order → process payment → persist outcome
- **`IOrderService.CapturePaymentAsync`** / **`RefundPaymentAsync`** — backoffice payment actions
- **`CheckoutResult`** — `PaymentSucceeded`, `Order`, `PaymentError`

### Checkout flow

1. Customer submits checkout form
2. Order is created with `PaymentStatus.Pending`
3. Active provider processes payment
4. **Success:** `OrderStatus.Processing`, `PaymentStatus.Authorized`, transaction ID stored, cart cleared, redirect to confirmation
5. **Failure:** `OrderStatus.Pending`, `PaymentStatus.Failed`, cart retained, error shown on checkout page

### Configuration (`appsettings.json`)

```json
"Payments": {
  "Provider": "dummy",
  "Currency": "SEK",
  "Dummy": {
    "SimulateFailure": false,
    "FailureMessage": "Payment declined (simulated)."
  }
}
```

Registered via `AddLumenPayments(configuration)` inside `AddLumenInfrastructure`.

### Storefront

- Checkout form includes a **Payment** section and optional failure simulation checkbox
- Submit button: "Place order & pay"
- Confirmation page shows payment status and transaction ID

### Backoffice

- Orders list includes payment status badge
- Order detail shows payment card (provider, transaction ID, captured/refunded amounts, message)
- **Capture payment** when status is `Authorized` or `Pending`
- **Refund** with amount input when payment is refundable

**Components:** `PaymentStatusBadge.razor`

## Adding a real provider (e.g. Stripe)

1. Implement `IPaymentProvider` in `Lumen.Infrastructure/Payments/`
2. Register with `services.AddSingleton<IPaymentProvider, StripePaymentProvider>()`
3. Add provider-specific options under `Payments` in configuration
4. Set `Payments:Provider` to the new provider name

## Verification checklist

- [x] `IPaymentProvider` abstraction exists and is registered via DI
- [x] Dummy provider works for successful and failed payments
- [x] Payment is called during checkout
- [x] Order status is updated correctly based on payment result
- [x] Error handling is graceful on the checkout page
- [x] Backoffice shows payment status on order details

## Run locally

```bash
dotnet ef database update --project src/Lumen.Infrastructure --startup-project src/Lumen.Backoffice
dotnet run --project src/Lumen.Storefront --urls http://localhost:5267
dotnet run --project src/Lumen.Backoffice --urls http://localhost:5258
```

**Try:**

1. Add a product to cart → `/checkout` → place order (payment succeeds) → confirmation shows transaction ID
2. Repeat with **Simulate payment failure** checked → error on checkout, cart retained
3. Use email containing `fail@` → payment fails without the checkbox
4. Backoffice **Orders** → open order → verify payment card, capture, and refund actions

## Post-phase fix

- **Logout redirect** (`f660c35`) — `AccountController.Logout` redirected to non-existent `Home/Index`; fixed to `LocalRedirect("/")` (home is served by `ContentController` at `/`).

## Commits

| Commit | Description |
|--------|-------------|
| `69c4472` | Phase 7: payment provider integration |
| `f660c35` | Fix storefront logout redirect |

## Out of scope (future phases)

- Real payment gateways (Stripe, Klarna, etc.) — abstraction is ready; implement `IPaymentProvider`
- Payment settings UI in backoffice Settings area
- Webhooks, 3-D Secure, saved payment methods
- Partial capture, multi-currency, payment emails