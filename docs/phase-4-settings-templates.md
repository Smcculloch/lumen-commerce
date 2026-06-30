# Phase 4 — Settings & Template Management

Phase 4 introduces a **Settings** area in the backoffice for managing templates by domain.

## Delivered capabilities

### Settings navigation

| Route | Purpose |
|-------|---------|
| `/settings` | Settings home with organized section cards |
| `/settings/templates/content` | List all content templates |
| `/settings/templates/content/new` | Create a dynamic content template |
| `/settings/templates/content/{key}` | View and manage a content template |
| `/settings/templates/product` | List all product templates |
| `/settings/templates/product/new` | Create a dynamic product template |
| `/settings/templates/product/{key}` | View and manage a product template |

**Nav:** Settings appears after Products in the sidebar.

### Template management service

- `ITemplateManagementService` — list, detail, dynamic template CRUD, property CRUD, reorder
- `ITemplateRepository` — persistence, usage counts, property-in-use checks
- Hybrid resolution unchanged: code-defined templates merge with database extensions via `ITemplateRegistry`

### Property editing rules

| Source | View | Edit | Delete |
|--------|------|------|--------|
| Code | Yes | No | No |
| Extension | Yes | Yes | Yes (blocked if in use) |
| Dynamic | Yes | Yes | Yes (blocked if in use) |

Adding a property to a code-defined template automatically creates an extension record in the database.

### Components

- `Components/Settings/TemplateSourceBadge.razor`
- `Components/Settings/PropertyDefinitionEditor.razor`
- `Components/Settings/TemplateDetailPanel.razor` — shared detail UI for both domains

## Verification checklist

- [x] Settings appears in the sidebar after Products
- [x] Settings home page shows organized sections
- [x] Content Templates can be viewed and edited in the backoffice
- [x] Adding/editing/deleting properties on a Content Template works correctly
- [x] Changes to templates are reflected when creating/editing content (via `ITemplateRegistry` merge)
- [x] Product Template management UI implemented (same patterns as Content)
- [x] UI follows existing backoffice patterns (banners, cards, tables, modals)

## Run locally

```bash
dotnet run --project src/Lumen.Backoffice
```

Open **Settings** → **Content Templates** → manage `standard-page` to see code + extension properties.