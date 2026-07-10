# Read Model & Catalog Pagination

## Context

The catalog (`GET /catalog`) is the synchronous read surface that the Client Portal
and Order Management System would call. It must return **approved, non-deleted**
products quickly, and the brief assumes a **large enterprise with a large customer
base** — so an unbounded "return every row" query is not acceptable.

## Decision

### 1. A dedicated CQRS read model

Reads are served from a denormalized `ProductReadModel` table, not the transactional
`Product`/`ProductVersion` tables:

- One row per product, representing its **current approved version** (name, price, sku,
  approver, approved-at, version provenance).
- Populated by a **projector** that writes the row in the same transaction as an
  approval, removes it on soft-delete, and re-projects it on restore.
- Exposed through a separate, read-only `ProductReadDbContext` that physically cannot
  see the write model — the CQRS seam is enforced by the type system, not convention.

Because only approved, live products ever exist in this table, the read API needs
**no filtering, no joins, and no workflow logic** — all of that work happens once, at
write time, so every read is cheap. This is the core scalability argument.

### 2. Keyset (seek) pagination

The catalog is paginated using **keyset pagination** over a monotonic `Sequence`
column, not offset (`Skip/Take`) pagination.

```
GET /catalog?after={cursor}&take={n}

WHERE Sequence > @after
ORDER BY Sequence
TAKE (n + 1)          -- one extra row tells us whether a next page exists
```

The response returns the page of items, the `NextCursor` (the last row's `Sequence`),
and `HasMore`. The client passes `NextCursor` back as `after` for the next page.

**Why keyset over offset:**

- **Constant-time reads at any depth.** `WHERE Sequence > cursor` is an indexed seek;
  page 10,000 costs the same as page 1. Offset (`OFFSET 100000`) must scan and discard
  the skipped rows.
- **No `COUNT` query.** `HasMore` is derived by fetching one extra row, so there is no
  separate (and increasingly expensive) count over a large table.
- **Capped `take`** (`Clamp(1..100)`) guarantees no single request can pull the whole
  table, regardless of catalog size.

**Trade-off:** keyset is forward-oriented ("next / load more"), not random-access
("jump to page 47"). This matches how a consumer *browses* a catalog and is the right
fit here. Numbered-page navigation (e.g. an internal admin grid) is the case where
offset pagination is preferable — a different tool for a different job.

## How the `Sequence` column behaves

`Sequence` is a database-generated identity, assigned on **insert**:

| Event | Effect on `Sequence` |
| --- | --- |
| First approval (row inserted) | Assigned the next value (1, 2, 3, …) |
| Re-approval (row upserted / updated) | **Unchanged** — the product keeps its original position |
| Soft-delete (row removed) | Value disappears, leaving a gap (harmless to keyset) |
| Restore / re-approve after delete (row re-inserted) | Gets a **new, higher** value — moves to the end |

The resulting order is "sequence of first entry into the catalog" — stable, unique, and
gap-tolerant, which is exactly what a keyset cursor needs.

## Sorting and filtering (future enhancement)

The single-`Sequence` cursor supports one fixed browse order. Richer querying evolves as
follows — deliberately **not** built now (less is more), but the foundation supports it:

### Filtering — a `WHERE` clause + an index

Keyset does **not** break under filtering. Predicates are added to the query and the
cursor still seeks on `Sequence`:

```sql
WHERE Sequence > @cursor AND Category = @cat AND Name LIKE @q
ORDER BY Sequence
```

The only requirement is a supporting composite index (e.g. `(Category, Sequence)`) so the
filtered seek stays fast.

### Sorting — the cursor becomes the sort key

Keyset seeks on the same column(s) it orders by. To sort by an arbitrary column, the
cursor must encode the **sort value + a unique tiebreaker**:

```sql
-- sort by Price ascending; cursor = (lastPrice, lastProductId)
WHERE Price > @lastPrice OR (Price = @lastPrice AND ProductId > @lastProductId)
ORDER BY Price, ProductId
```

Each supported sort option needs its own composite cursor and a supporting composite
index (`(Price, ProductId)`, `(Name, ProductId)`, …). The `Sequence` cursor is simply the
special case where the sort key *is* the sequence.

### Rich search — evolve the read store

Once the catalog needs arbitrary multi-column sort, faceted filtering, full-text search,
and relevance ranking at scale, hand-rolled keyset over SQL stops being the right tool.
At that point the **read store itself evolves**: project approved products into a
purpose-built search index (e.g. Azure Cognitive Search / Elasticsearch) instead of, or
alongside, the SQL table.

This is the payoff of CQRS: the read side is a separate, repointable projection. Changing
the read store is a **projector-target change** — the write model, approval workflow, and
API contract are untouched. Sort/filter/search is a read-store concern, and CQRS isolates
it there.

## Summary

- **Now:** denormalized SQL read model + keyset pagination on `Sequence` — constant-time,
  bounded, no count. Serves the demo and the "large customer base" requirement.
- **Next (documented, not built):** composite-cursor keyset for a few sort options;
  filters as indexed `WHERE` predicates; and, at real scale, a dedicated search index
  reached by re-pointing the projector — no upstream rewrite.
