# Lumen Commerce

A modern .NET 8 modular monolith for e-commerce, inspired by Optimizely Commerce and Litium. Lumen combines CMS content management and PIM product information in a single platform with a shared **hybrid template system**.

## Solution Structure

```
LumenCommerce/
├── src/
│   ├── Lumen.Domain/           Core entities, value objects, enums
│   ├── Lumen.Application/      Services, DTOs, template definitions
│   ├── Lumen.Infrastructure/   EF Core, repositories, integrations
│   ├── Lumen.Backoffice/       Blazor Server admin UI (scaffold)
│   ├── Lumen.Storefront/       ASP.NET Core MVC public site (scaffold)
│   └── Lumen.Shared/           Constants, extensions, shared kernel
├── tests/                      (reserved for Phase 2+)
├── docs/
└── LumenCommerce.sln
```

### Layer Responsibilities

| Project | Role |
|---------|------|
| **Lumen.Domain** | Thin domain model: `ContentItem`, `Product`, template metadata types |
| **Lumen.Application** | Use cases, `ITemplateRegistry`, validation, code-first templates |
| **Lumen.Infrastructure** | SQLite persistence, EF mappings, registry implementation |
| **Lumen.Backoffice** | Future admin UI for template extension and content/product editing |
| **Lumen.Storefront** | Future public rendering and commerce flows |
| **Lumen.Shared** | Cross-cutting constants and helpers |

## Architecture Vision

Lumen is a **modular monolith**: one deployable application split into clear bounded contexts (CMS, PIM, Commerce, Backoffice) that share infrastructure but keep domain logic isolated. The template system is the architectural spine — the same metadata model powers both editorial content and merchandised products.

```
┌─────────────────────────────────────────────────────────────┐
│                    Lumen.Backoffice                          │
│                    Lumen.Storefront                          │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                   Lumen.Application                          │
│  IContentService · IProductService · ITemplateRegistry     │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                  Lumen.Infrastructure                        │
│         AppDbContext · TemplateRegistry · Repositories       │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                     Lumen.Domain                             │
│   TemplateDefinition · ContentItem · Product · PropertyBag   │
└─────────────────────────────────────────────────────────────┘
```

## The Hybrid Template System

### Core Concepts

| Concept | Description |
|---------|-------------|
| **Template Definition** | Describes the *shape* of content or products (fields, types, validation) |
| **Template Instance** | A concrete `ContentItem` or `Product` with property values |
| **Property Definition** | A single field: name, type, required flag, validation rules |
| **Resolved Template** | Code + database definitions merged at runtime |

### Definition vs Instance

- **Definition** (`ContentTemplateDefinition`, `ProductTemplateDefinition`, `ResolvedTemplateDefinition`) — metadata only, no editorial values.
- **Instance** (`ContentItem`, `Product`) — stores values in a case-insensitive property bag (`Dictionary<string, object?>`), serialized to JSON in the database.

### Hybrid Model (Code + Backoffice)

Lumen supports three template origins:

1. **Code-first** — compiled templates discovered via attributes or fluent builders. Preferred for core platform types.
2. **Extensions** — additional properties stored in the database that *extend* a code template (same `key`).
3. **Fully dynamic** — templates created only in the backoffice with no code counterpart.

At runtime, `TemplateRegistry` merges code and database definitions:

```
Code Template (standard-page)
    + DB Extension (seoKeywords, promoBanner)
    = ResolvedTemplateDefinition
```

**Trade-offs of this approach:**

| Benefit | Cost |
|---------|------|
| Strong typing and compile-time safety for core templates | Extended fields are not strongly typed in C# |
| Merchants can add fields without redeploying | Property bags lose IDE autocomplete for dynamic fields |
| Single registry API for all consumers | Merge rules must stay predictable (code wins on name collision) |
| Easy seeding and migration of extensions | Backoffice UI must render fields from metadata |

On property name collision, **code-defined properties take precedence** over database extensions.

### Supported Property Types

`Text`, `RichText`, `Html`, `Number`, `Integer`, `Decimal`, `Boolean`, `DateTime`, `Image`, `MediaReference`, `Reference`, `Select`, `MultiSelect`, `Json`

## Creating a Template in Code

### Option A: Attribute-Driven (POCO)

```csharp
[ContentTemplate("article-page", "Article Page")]
public sealed class ArticlePageTemplate
{
    [TemplateProperty(PropertyType.Text, DisplayName = "Headline", IsRequired = true)]
    public string Headline { get; set; } = string.Empty;
}
```

Templates are discovered automatically from the `Lumen.Application` assembly via `AttributeTemplateDiscovery`.

### Option B: Fluent Builder

```csharp
var template = TemplateBuilder<ProductTemplateDefinition>
    .ForProduct("standard-product", "Standard Product")
    .AddProperty("price", "Price", PropertyType.Decimal, p => p.Required().MinValue(0))
    .Build();
```

Register fluent templates in `LumenCodeTemplateProvider`.

### Shipped Templates (Phase 1)

| Key | Kind | Definition Style |
|-----|------|------------------|
| `standard-page` | Content | Attributes (`StandardPageTemplate`) |
| `article-page` | Content | Attributes (`ArticlePageTemplate`) |
| `standard-product` | Product | Fluent builder (`StandardProductTemplate`) |

Seed data also demonstrates:

- **Extension** on `standard-page` → `seoKeywords`, `promoBanner`
- **Dynamic template** → `landing-page`

## Using Templates in Application Services

```csharp
// Resolve merged template
var template = await _templateRegistry.GetByKeyAsync("standard-page");

// Validate and hydrate property values
var hydrated = _validator.Hydrate(template, request.Properties);

// Create instance
var item = ContentItem.Create(template.Key, request.Name, hydrated);
await _contentItemRepository.AddAsync(item);
```

`IContentService` and `IProductService` encapsulate this flow for CMS and PIM respectively.

## CMS Module (Phase 2)

The CMS module builds on the template system with hierarchical content management.

### Content Model

`ContentItem` now includes:

| Field | Purpose |
|-------|---------|
| `ParentId` | Adjacency-list parent reference for the content tree |
| `SortOrder` | Sibling ordering |
| `Level` | Depth in the tree (0 = root) |
| `MaterializedPath` | GUID path (`/{guid}/{guid}/`) for efficient descendant queries |
| `FullPath` | Slug path (`/home/about`) for URL routing |
| `Status` | `Draft`, `Published`, or `Archived` |
| `PublishedAt` | Timestamp set on publish |

Property values remain in a JSON property bag validated against the resolved template (including DB extensions).

### CMS Services

`IContentService` provides:

- `GetContentById` / `GetContentBySlug` (matches `FullPath`)
- `GetRoots` / `GetChildren` / `GetDescendants`
- `CreateContent` / `UpdateContent` — validates properties via `TemplateValidator`
- `PublishContent` / `UnpublishContent` — publish re-validates required fields
- `DeleteContent` — blocked when child nodes exist

`IMediaRepository` and `MediaItem` provide a basic media library foundation (upload UI deferred).

### Content Tree Strategy

**Adjacency list** (`ParentId`) plus **materialized GUID path** for descendants. Slug-based `FullPath` is maintained on create/update and cascades to descendants when a node moves or is renamed. Slugs are unique among siblings; `FullPath` is globally unique.

### Seeded Content

On first run, a published **Home** page is seeded using the `standard-page` template (including extension fields `seoKeywords` and `promoBanner`).

### Backoffice CMS (Phases 2.5 + 2.6)

The Blazor backoffice includes a functional hierarchical content management UI:

| Route | Purpose |
|-------|---------|
| `/content` | **Tree view** (default) or list view — expand/collapse, search, actions per node |
| `/content/new` | Create root content — pick template, optional parent selector |
| `/content/new?parentId={id}` | Create child under a parent (pre-filled in editor) |
| `/content/edit/{id}` | Edit with dynamic template-driven form |

**Tree view features:** nested nodes with connector lines, expand/collapse all, Tree/List toggle, search auto-expands matching branches.

**Seeded hierarchy:** `Home` (root) → `Privacy` (child at `/home/privacy`).

The editor renders form fields from `ResolvedTemplateDefinition.Properties` (code-defined + DB extensions).

```bash
dotnet run --project src/Lumen.Backoffice
# → http://localhost:5258/content
```

### Migrations

Phase 2 uses EF Core migrations instead of `EnsureCreated`:

```bash
cd src/Lumen.Backoffice
dotnet ef database update --project ../Lumen.Infrastructure/Lumen.Infrastructure.csproj
```

In Development, startup automatically recreates legacy databases that were built with Phase 1's `EnsureCreated` (tables exist but no migration history). You can also delete `*.db` files manually if needed.

## Database

**SQLite** with EF Core migrations. Tables:

- `TemplateDefinitions` / `PropertyDefinitions` — extensions and dynamic templates
- `ContentItems` / `MediaItems` / `Products` — CMS, media, and PIM data

Both apps share a single development database so content published in the backoffice is visible on the storefront:

- Default: `Data Source=../../data/lumencommerce.db` (relative to each project folder)

Override via `ConnectionStrings:DefaultConnection` in `appsettings.json`.

### Storefront CMS rendering

The storefront renders **published** content only:

- `/` and `/home` → published page at FullPath `/home`
- `/{slug}` → other published pages (e.g. `/about`)

Draft content remains backoffice-only until published.

## Running Locally

```bash
cd LumenCommerce

# Backoffice (Blazor Server)
dotnet run --project src/Lumen.Backoffice

# Storefront (MVC)
dotnet run --project src/Lumen.Storefront
```

## Extending Templates from the Backoffice (Future)

The schema and seed data are in place. A later phase will add UI to:

1. Browse code-defined templates (read-only base properties)
2. Add extension properties to an existing template key
3. Create fully dynamic templates
4. Edit select options and validation rules

The persistence model (`TemplateDefinitionEntity`, `PropertyDefinitionEntity`) is already in place.

## Planned Phases

| Phase | Focus |
|-------|-------|
| **0 + 1** | Solution foundation, hybrid template system |
| **2** (current) | CMS module — content tree, publishing, media foundation |
| **3** | PIM module — variants, categories, pricing rules |
| **4** | Commerce — cart, checkout, orders |
| **5** | Backoffice UI — template editor, content/product workspaces |
| **6** | Storefront rendering — template-driven pages, search |

## Key Design Decisions

1. **Property bag instances** — flexible enough for dynamic fields; strongly typed accessors can be added per template in later phases.
2. **Registry merge at read time** — no duplication of code templates in the database.
3. **SQLite for Phase 1** — zero-config dev experience; swap to SQL Server/PostgreSQL later via connection string.
4. **Validation in Application layer** — `TemplateValidator` is reusable by Backoffice and Storefront.
5. **Separate DB files per host** — avoids file-lock conflicts during parallel dev; use a shared database in production.