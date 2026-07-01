# Phase 8 — Advanced Backoffice Order Management + Scheduled Jobs

Phase 8 expands backoffice order management with advanced filtering, status workflow with notes and timeline, and introduces Quartz.NET scheduled background jobs with a management UI.

## Delivered capabilities

### Advanced order management

**Orders list (`/orders`)**

- Search (order number, email, name)
- Filter by order status, payment status, date range
- Filter by customer via `?customerId={guid}` (linked from customer detail)
- Payment status column

**Order detail (`/orders/{id}`)**

- Full line items, addresses, payment card (from Phase 7)
- **Status update with optional note** — recorded in order timeline
- **Order timeline** — `OrderHistoryEntry` events (created, status, payment, notifications, cancelled)
- **Actions:** cancel order, send notification (stub), capture/refund payment

**Components:** `OrderTimeline.razor`

### Domain & persistence

- **`OrderHistoryEntry`** — audit trail per order
- **`JobExecution`** — background job run log
- **`NotificationLog`** — stub outbound messages (abandoned cart, order status)
- Migration: `Phase8_ScheduledJobs`

### Scheduled jobs (Quartz.NET)

Registered in backoffice only via `AddLumenScheduledJobs(configuration)`.

| Job key | Purpose |
|---------|---------|
| `abandoned-cart` | Queues reminder notifications for inactive carts with items (customer email required) |
| `cart-cleanup` | Deletes stale empty carts past configured age |
| `order-status` | Queues status reminders for long-pending/processing orders |
| `inventory-sync` | Stub for future ERP/PIM inventory sync |

**Jobs UI (`/jobs`)**

- List registered jobs with cron schedule
- Last run status and message
- **Run now** manual trigger
- Recent execution history

Notifications are written to `NotificationLogs` — no SMTP/email provider configured.

### Configuration (`appsettings.json` — Backoffice)

```json
"Jobs": {
  "Enabled": true,
  "AbandonedCart": {
    "InactiveHours": 24,
    "CronExpression": "0 0 * * * ?"
  },
  "CartCleanup": {
    "MaxAgeDays": 30,
    "CronExpression": "0 30 2 * * ?"
  },
  "OrderStatus": {
    "PendingHours": 48,
    "CronExpression": "0 0 6 * * ?"
  },
  "InventorySync": {
    "CronExpression": "0 0 3 * * ?"
  }
}
```

Set `Jobs:Enabled` to `false` to disable the Quartz scheduler (manual trigger will not work).

### Application services

- `IOrderService.UpdateStatusWithNoteAsync`, `CancelOrderAsync`, `SendOrderNotificationAsync`, `GetOrderHistoryAsync`
- `IJobRunner` — list jobs, recent executions, trigger manually
- `OrderListFilter` extended with `PaymentStatus`, `CustomerId`

## Architecture notes

- Job handlers live in `Lumen.Infrastructure/Jobs/Handlers/`; Quartz wrappers in `Jobs/Quartz/`
- `JobExecutionLogger` records each run to SQLite
- Storefront does not host the scheduler — only shared job service registrations for order history
- Extending jobs: add handler + Quartz job + register in `JobServiceCollectionExtensions`

## Verification checklist

- [x] Backoffice Orders section has advanced filtering and detail view
- [x] Order status can be updated with notes
- [x] Scheduled jobs system is integrated and working
- [x] Example jobs (abandoned cart, cleanup, etc.) run correctly
- [x] Jobs management UI exists in the backoffice
- [x] System follows previous architectural patterns

## Run locally

```bash
dotnet ef database update --project src/Lumen.Infrastructure --startup-project src/Lumen.Backoffice
dotnet run --project src/Lumen.Backoffice --urls http://localhost:5258
```

**Try:**

1. Backoffice **Orders** — filter by status, payment, date range
2. Open an order — change status with a note, view timeline
3. Cancel an order or send notification (stub)
4. **Jobs** — run `cart-cleanup` or `inventory-sync` manually, check execution log
5. Customer detail → **View orders** (customer filter)

Storefront: `http://localhost:5267` | Backoffice: `http://localhost:5258`

## Out of scope (future phases)

- Real email/SMS delivery
- Advanced reporting and analytics
- Job editing UI (cron changes require config)
- Inventory sync with external systems