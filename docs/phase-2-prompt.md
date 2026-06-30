# Suggested Prompt for Phase 2 (CMS Module)

Copy and use this prompt to continue building Lumen Commerce:

---

You are continuing work on **Lumen Commerce** at `C:\Projects\Home\LumenCommerce`.

Phase 0 + Phase 1 are complete. The hybrid template system, `AppDbContext`, template registry, and code-first templates (`standard-page`, `article-page`, `standard-product`) are in place.

## Phase 2 Goal: CMS Module

Implement the core CMS bounded context on top of the existing template foundation.

### Requirements

1. **Content Tree / Hierarchy**
   - Adjacency list (`ParentId`) + `MaterializedPath` (GUID segments) + slug-based `FullPath`
   - `IContentService` with `GetRoots`, `GetChildren`, `GetDescendants`, breadcrumbs via path
   - Sibling `SortOrder`; slug unique among siblings; `FullPath` globally unique

2. **Publishing Workflow**
   - `ContentStatus`: Draft, Published, Archived
   - `PublishedAt` set on publish; publish re-validates required template properties
   - `DeleteContent` blocked when children exist

3. **URL / Slug Management**
   - `FullPath` built from parent path + slug (e.g. Home → Privacy = `/home/privacy`)
   - Auto-slug from name when omitted

4. **Media Library (Basic)**
   - `MediaItem` entity + repository (upload UI deferred)

5. **Backoffice CMS (Blazor Server)**
   - `/content` — **Tree/List toggle**
   - **Tree mode:** split explorer — left = hierarchical tree (expand/collapse, search), right = **read-only preview** of selected page with action toolbar
   - **List mode:** table with indented rows and per-row actions
   - `/content/new`, `/content/edit/{id}` — dynamic form from `ResolvedTemplateDefinition`
   - Template query params loaded in `OnParametersSetAsync` (required for landing-page and other templates)
   - Nav: Dashboard, then Content

6. **Storefront**
   - `ContentController` renders published content by `FullPath`
   - `/` → home; no `HomeController.Privacy` scaffold conflicting with `/home/privacy`
   - Remove any guard that returns 404 for lowercase `home/*` CMS paths

7. **Shared Database**
   - `DatabasePath.Resolve(contentRootPath)` → `LumenCommerce/data/lumencommerce.db`
   - Both Backoffice and Storefront use absolute path; log path in Development

8. **Seed Data**
   - Home (root, published) → Privacy (child at `/home/privacy`, published)

### Implementation Constraints (learned from Phase 2)

- **Do not** use a recursive Blazor `.razor` component for the content tree — render inline in `Index.razor` via `ContentTreeHelper.FlattenVisible`
- **Do not** use CWD-relative `../../data/...` connection strings without `DatabasePath`
- **Do not** nest multiple Bootstrap cards inside the preview pane — use one toolbar + content block
- Use `ContentRepository.UpdateAsync` with tracked entity (`FindAsync` + `SetValues`), not `Update(detached)`

### Rules

- Reuse `ITemplateRegistry`, `TemplateValidator`, and existing entities — extend, don't rewrite
- Keep PIM/commerce out of scope
- Update `docs/phase-2-full-cms.md` with any scope changes
- End with summary and verification against the checklist in `phase-2-full-cms.md`

Begin now.

---

## Reference

See **`docs/phase-2-full-cms.md`** for the merged deliverable list, file map, bug-fix checklist, and verification steps.