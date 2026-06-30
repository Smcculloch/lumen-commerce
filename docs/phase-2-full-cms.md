# Phase 2 — Full CMS (Merged Definition)

Phases 2, 2.5, and 2.6 together deliver a complete CMS foundation. Use this document as the **canonical scope and replay guide** when re-implementing or extending Phase 2.

## Delivered Capabilities

### Domain & Services (Phase 2)

- `ContentItem` with hierarchy (`ParentId`, `SortOrder`, `Level`, `MaterializedPath`, `FullPath`)
- `ContentStatus` — `Draft`, `Published`, `Archived`
- `IContentService` — CRUD, publish/unpublish, tree queries (`GetRoots`, `GetChildren`, `GetDescendants`)
- `MediaItem` entity + `IMediaRepository` (foundation only; no upload UI)
- EF Core migrations (`Phase2_CmsModule`); legacy `EnsureCreated` databases recreated in Development
- **Shared database resolution** via `DatabasePath.Resolve(contentRootPath)` so Backoffice and Storefront always use `LumenCommerce/data/lumencommerce.db` regardless of process working directory

### Seeded Content

| Page | Parent | FullPath | Status |
|------|--------|----------|--------|
| Home | *(root)* | `/home` | Published |
| Privacy | Home | `/home/privacy` | Published |

Seed logic is upgrade-friendly: adds Home/Privacy if missing without wiping existing data.

### Backoffice UI (Phases 2.5 + 2.6)

| Route | Purpose |
|-------|---------|
| `/content` | CMS explorer — **Tree** (default) or **List** |
| `/content/new` | Create root page — template picker |
| `/content/new?template={key}` | Create with template (loads fields in `OnParametersSetAsync`) |
| `/content/new?parentId={id}` | Create child under parent |
| `/content/edit/{id}` | Dynamic template-driven editor |

**Nav menu order:** Dashboard first, then Content.

#### Tree mode — split explorer (left tree + right preview)

Inspired by Blazorise TreeView UX (compact hierarchy, selection, expand/collapse) but implemented without a third-party package:

- **Left pane — Site structure**
  - Compact tree rows with chevron expand/collapse, folder/file icons, status dots
  - Click a node to select it; search auto-expands matching branches
  - Expand all / Collapse all
- **Right pane — Read-only preview**
  - Single compact toolbar: page name, status, path/template/slug/dates, action buttons (Edit, Add child, Publish/Unpublish, Delete)
  - Rendered content: title (if different from name), introduction, HTML body
  - Remaining template fields in a collapsible **Template properties** section (excludes fields already shown in preview)

**Important implementation note:** Do **not** use a self-referencing recursive Blazor component for the tree — it fails to render under Blazor Server prerendering. Render the tree inline in `Pages/Content/Index.razor` using `ContentTreeHelper.FlattenVisible` (same data path as list view).

**Key components:**

| File | Role |
|------|------|
| `Components/Content/ContentTreeHelper.cs` | Children lookup, flatten for list/tree, search visibility |
| `Components/Content/ContentPreviewPanel.razor` | Read-only preview pane |
| `Components/Content/ContentPreviewHelper.cs` | Title/body extraction from property bag |
| `Components/Content/TemplatePropertyForm.razor` | Dynamic editor fields |
| `Pages/Content/Editor.razor` | Create/edit — template loading in `OnParametersSetAsync` (not `OnInitializedAsync` alone) |

#### List mode

Table view with indented rows, same expand/search semantics, full per-row actions. Use for bulk browsing.

### Storefront

- `ContentController` renders published CMS pages by `FullPath`
- `/` → `/home`; `/home` explicit route; `/{**slug}` for nested paths (e.g. `/home/privacy`)
- **No scaffold route collision:** remove `HomeController.Privacy` and `Views/Home/Privacy.cshtml` — they case-insensitively matched `/home/privacy` and showed static placeholder content instead of CMS data
- Do **not** block `home/*` paths in `ContentController` — that prevented CMS child pages from resolving

### Database

- Canonical file: `LumenCommerce/data/lumencommerce.db`
- Both apps call `DatabasePath.Resolve(builder.Environment.ContentRootPath)` in `Program.cs`
- Development startup logs: `[Lumen] SQLite database: {absolute path}`
- Delete stray per-project `*.db` files (`lumen-backoffice.db`, `lumen-storefront.db`, etc.) if created before the shared-path fix

### CSS (Backoffice)

Tree and preview styles in `wwwroot/css/site.css`:

- `.cms-treeview*` — compact tree rows
- `.content-explorer*` — split-pane layout
- `.content-preview-*` — single-pane preview (no stacked cards); reduced empty-state height

## Bugs Fixed During Phase 2 (replay checklist)

1. **Backoffice empty / wrong content** — multiple SQLite files from CWD-relative connection strings → `DatabasePath.Resolve`
2. **Landing page “Loading template fields…”** — `[SupplyParameterFromQuery] Template` not ready in `OnInitializedAsync` → load in `OnParametersSetAsync`
3. **Privacy page wrong on storefront** — `Home/Privacy` scaffold route + `ContentController` `home/*` 404 guard
4. **Tree view blank** — recursive `ContentTreeNode` component → inline tree in `Index.razor`
5. **Preview whitespace** — merged meta + body cards into one toolbar + content block; removed `min-height` on preview pane
6. **EF tracking on publish** — `ContentRepository.UpdateAsync` uses `FindAsync` + `SetValues`
7. **Legacy DB vs migrations** — `DatabaseInitializer.MigrateAsync` deletes orphan tables in Development, then migrates

## Not Yet in Scope

- Drag-and-drop reorder
- Media library UI
- Workflow / versioning
- Dynamic storefront navigation from content tree
- Permissions
- Third-party TreeView package (e.g. Blazorise) — optional future enhancement

## Verification Checklist

After implementing Phase 2, confirm:

- [ ] Both apps log the same `data/lumencommerce.db` path on startup
- [ ] Backoffice `/content` tree shows **Home → Privacy**; selecting Privacy updates preview
- [ ] List toggle shows the same pages in a table
- [ ] Storefront `/` and `/home/privacy` render seeded CMS HTML
- [ ] Creating a **Landing Page** shows `headline`, `ctaText`, `theme` fields
- [ ] Publish from editor returns to content list with success banner

## Suggested Next Step

Proceed to **Phase 3 (PIM Module)** per `docs/phase-3-prompt.md`, or extend storefront with CMS-driven navigation.