# Architecture & Design Decisions

Product Management System — MOYO Online Order Solution (interview project).
Backend-focused slice. Stack: C# / .NET 10 (ASP.NET Core Web API), EF Core (Code-First), SQL Server/LocalDB, Angular, OAuth 2.0 / OIDC, Azure PaaS.

_Last updated: 2026-07-01_

---

## 1. System choice — Product Management System

**Decision:** Build the Product Management System slice rather than the Client Portal or Order Management System.

**Why:**
- It is **self-contained** — synchronous, and always the *callee* (the Portal and OMS call it). It can be built and demoed end-to-end without a message bus or standing up the other two systems.
- Its spec is the only one that flags **"Authorisation functionality important"** — it rewards exactly the backend skills being graded.
- One cohesive slice lets me show role-based authorization, an approval workflow/state machine, soft-delete, and a Data Lake CQRS read model together.

---

## 2. Layered architecture (Api / Application / Domain / Infrastructure) over plain MVC

**Decision:** Structure the solution in clean-architecture layers, not a flat Model/View/Controller project.

**Why:**
- MVC is a *presentation* pattern (how one request is handled at the UI boundary); it says nothing about where business rules live or which way dependencies point. In an API there is no View anyway. Clean architecture is an *application-structure* pattern and the two compose — Controllers live in the Api layer.
- **The dependency rule:** dependencies point inward. `Domain` (entities, workflow rules) depends on nothing — not EF Core, not ASP.NET, not Azure. The most valuable, hardest-to-rewrite code is insulated from volatile frameworks.
- **Swappability at the seams:** the Data Lake read store and the identity provider (Entra today, Duende as fallback) are Infrastructure concerns behind Domain-defined interfaces; the Domain never knows the implementation.
- **Testability:** the workflow state machine and authz handler are pure, framework-free logic — unit-testable without a web host or database.
- It is the headline the brief grades: *"architecture is critical"*, *"assume large enterprise."*

**Trade-off acknowledged:** for a CRUD app this small, layering is arguably over-engineering. Justified by the brief's explicit *"assume future enhancements / large customer base"* — demonstrating the structure that scales, on a slice small enough to stay polished.

**Layer responsibilities:**
- `Api` — controllers, auth policies, DTOs, mapping, DI wiring (the only layer that knows everything).
- `Application` — handlers/services, workflow logic, validation.
- `Domain` — entities, enums, domain rules, interfaces.
- `Infrastructure` — EF Core DbContext, migrations, read-store projection.

---

## 3. Layers as folders (within one project), not separate projects

**Decision:** Implement the layers as **folders inside the `SystemChallengeAPI` project** rather than four separate class-library projects.

**Why:** pragmatic for a slice this size — fewer moving parts, faster to build.

**Trade-off accepted:** with everything in one assembly, the "Domain depends on nothing" rule is enforced by **discipline and folder boundaries, not the compiler**. Separate projects would enforce it physically (Domain could not reference Infrastructure because no project reference exists). If asked "what keeps your domain clean?", the honest answer here is convention, not the compiler.

---

## 4. Domain model — `Product` (aggregate root) + immutable `ProductVersion` rows

**Decision:** One stable `Product` row, many `ProductVersion` rows.

- **`Product` = the stable aggregate root / identity.** Holds what persists across edits: `Id` (what the read store & OMS reference), soft-delete fields, creation audit, and a `CurrentApprovedVersionId` pointer. It holds *no* product attributes.
- **`ProductVersion` = an immutable snapshot of the attributes + its own workflow state.** Name, Description, Price, Sku, `Status`, authoring audit, and decision metadata (`DecidedBy/At`, `DecisionReason`).

**Why one-row-plus-versions:** clean approval history and a full audit trail — no destructive updates.

### 4a. `WorkflowStatus` lives on the *version*, not the product

**Decision:** Status is per-version.

**Why:** the workflow acts on a *change*, not the product as a whole. When a Capturer edits an already-approved product, a new `Pending` version is created while the old `Approved` version **stays live and keeps serving Portal/OMS**. If status lived on `Product`, editing would flip the whole product to Pending and there would be nothing approved to serve. Per-version status lets the live version and the in-flight edit coexist.

### 4b. Immutability rule

**Decision:** A version's **attributes freeze the moment it leaves `Draft`.** After submission, Name/Price/etc. never change; only the status transitions and approval metadata is stamped.

**Why:** strict immutability is impossible (status must change), so the precise, defensible rule is "attribute-frozen after submission." Preserves every approved/rejected version as immutable history.

### 4c. Lifecycle

1. **Create** → `Product` + Version 1 @ `Draft` (the single editable scratchpad).
2. **Edit while Draft** → mutate in place (not yet submitted, not yet frozen).
3. **Submit** → `Draft → Pending` (now frozen).
4. **Approve** (Manager) → `Pending → Approved`, stamp decision, set `Product.CurrentApprovedVersionId`, project to read store.
5. **Reject** (Manager) → `Pending → Rejected` with a reason (frozen history).
6. **Edit an approved product** → new `Draft` copy → submit → Pending → approve → new live version. Old approved versions remain as immutable history.

### 4d. `CurrentApprovedVersionId` is a denormalized pointer

**Decision:** Keep an explicit "what's live" pointer on `Product`.

**Why / caveat:** it is a read optimization. The version rows are the source of truth ("what's live" is derivable as the latest `Approved` version); the pointer just makes lookups fast. Defensible as denormalization, not redundancy.

### 4e. Enums

- `WorkflowStatus`: `Draft → Pending → Approved | Rejected`.
- `UserRole`: `Capturer` (Create/Read/Update), `Manager` (Approve/Reject, soft-delete, + everything Capturer can do).

---

## 5. Persistence — EF Core Code-First

**Decision:** EF Core 10 Code-First against SQL Server / LocalDB.
Packages: `Microsoft.EntityFrameworkCore`, `.Design`, `.SqlServer` (10.0.9).

- **`ApplicationDbContext`** in the Infrastructure folder.
- **DbSets** exposed as `public DbSet<T> ... => Set<T>();` — public, PascalCase, no nullable backing field.
- **Entity configuration** via `OnModelCreating` for now; will migrate to `IEntityTypeConfiguration<T>` classes as the model grows (keeps each entity's mapping isolated and testable — the idiomatic form for an architecture-graded project).

### 5a. Circular foreign key

**Decision:** Two relationships exist between the tables — `Product` → many `ProductVersion` (via `ProductId`), and `Product.CurrentApprovedVersionId` → one `ProductVersion`. The first uses `DeleteBehavior.Cascade`; the second uses **`DeleteBehavior.NoAction`**.

**Why:** if both cascaded, SQL Server rejects the schema (cascade cycle). `NoAction` on the pointer breaks the cycle. `CurrentApprovedVersionId` is nullable → an optional relationship (no live version until first approval). Verify in the generated migration: exactly one `ON DELETE CASCADE` and one `ON DELETE NO ACTION`.

### 5b. Column mapping

- `WorkflowStatus` stored **as a string** (`HasConversion<string>`) for readability in the DB.
- `Price` → `HasPrecision(18, 2)` (avoid float rounding on money).
- `Name` (200) / `Sku` (64) → max length + required.
- A composite **unique** index on `(ProductId, VersionNumber)` — enforces version-number integrity at the schema level and covers `ProductId`-only lookups (leftmost prefix), replacing the standalone `ProductId` index.

### 5c. Primary keys — `Guid` keys with client-side sequential generation

**Decision:** `Guid` primary keys on `Product` and `ProductVersion`, generated client-side (EF Core's default `SequentialGuidValueGenerator` for `Guid` keys; `Guid.NewGuid()` conceptually).

**Why:**
- **Client-side identity** — the aggregate has its Id the moment it is constructed, with no database round-trip (good for DDD and for later publishing events).
- **Non-enumerable** — sequential `int` keys would leak product counts to the Portal/OMS and allow ID enumeration; GUIDs do not.
- **Distributed-safe uniqueness** — no coordination needed across creation paths.

**Trade-off acknowledged (the scale question):** GUIDs as the clustered PK risk **index fragmentation / page splits** — random GUIDs insert at random B-tree positions, and the wide 16-byte key inflates every non-clustered index. Mitigation: EF Core generates **sequential** GUIDs client-side by default, ordered to minimise page splits (not byte-identical to SQL Server's `NEWSEQUENTIALID`, so "much better than random," not perfect).

**Alternative considered — `bigint` identity (rejected for this slice):** tightest clustered index, but forces a DB round-trip for identity and leaks enumeration. The enterprise hybrid — `bigint` internal clustered key + `Guid` external/public key — is the escalation path if write throughput ever became the bottleneck. Kept a single `Guid` key here for simplicity.

**Note on seed GUIDs:** seed rows use hard-coded, patterned GUIDs (`1111…`, `aaaa…0001`) because `HasData` requires static values (a generated value would churn a new migration each build). They are deterministic, not random — correct and intentional for seeding; runtime records use generated values.

---

## 6. Soft-delete via a global query filter

**Decision:** `Product` carries `IsDeleted` / `DeletedAtUtc` / `DeletedBy`, plus a global query filter `HasQueryFilter(p => !p.IsDeleted)`.

**Why — retention and default-visibility are orthogonal:**
- **Soft-delete (the flag) = retention.** The row stays in the table forever; this is what makes auditing possible.
- **The query filter = default visibility.** It only changes what a *normal* query returns; it deletes/hides nothing permanently.

Together they invert the default to the **safe** direction: deleted products are excluded everywhere unless explicitly requested, so they can never accidentally surface in the Portal/OMS read model. Without the filter, every query everywhere must remember `.Where(p => !p.IsDeleted)` — miss one and a deleted product leaks.

**Auditing / restore** uses **`IgnoreQueryFilters()`** to opt back in on purpose:
```csharp
var deleted = await db.Products.IgnoreQueryFilters()
    .Where(p => p.IsDeleted).ToListAsync();
```

**Known asymmetry:** the filter is on `Product` only; `ProductVersion` has none, so a raw `ProductVersions` query can return versions of a deleted product. EF applies the `Product` filter automatically only when navigating *through* `Product`. Mitigation: query versions via the product. Left as-is, consciously.

---

## 7. CQRS / Data Lake read model (planned)

**Decision:** A separate denormalized read store for approved products; on Approve, project the approved version into it; serve a synchronous public "Get Products" read API from it (what Portal/OMS call).

**Why:** read/write separation and fast retrieval for a large customer base; the read API never exposes soft-deleted or unapproved items. Keep pragmatic (a read table/schema) — do not overbuild.

---

## 8. Identity & Authorization

### 8a. Identity lives in the identity provider, not the application database

**Decision:** Use OAuth 2.0 / OIDC with an **external identity provider** — **Microsoft Entra ID (chosen)**; Duende IdentityServer was considered as a self-hosted local alternative. Users, passwords, and role assignments live in the IdP — the application database holds **only** `Product` and `ProductVersion`, no `Users`/`Roles` tables. Roles are delivered as Entra **App roles** (`Capturer` / `Manager`), surfaced in the token's `roles` claim.

**Why:** on each request the API validates a signed **JWT bearer token** and reads its **claims** (identity + role). Roles are not a table to query; they are a claim the API trusts because the token is signed by a configured issuer. This is the enterprise-correct model — a large org runs central identity (Entra), it does not give every service its own user store. Aligns with the brief's *"assume large enterprise"* and its emphasis on OAuth 2.0 / OIDC.

**How identity touches the DB:** as a **reference, not an owned entity.** The audit fields (`CreatedBy`, `DeletedBy`, `DecidedBy`) are `string` — they store the user's stable identifier from the token (`sub` / email), stamped straight off the request. There is deliberately no foreign key to a user table.

**Note on the `UserRole` enum:** it is **not persisted** — nothing in the DB references it. With an external IdP, roles arrive as string claims. The enum's job is on the authorization side: strongly-typed role names / policy constants for parsing and matching claims, not a stored column.

**Alternative considered — ASP.NET Core Identity (rejected):** would put `AspNetUsers` / `AspNetRoles` / `AspNetUserRoles` in the application DB and manage accounts/passwords in-app. Self-contained and easier to demo, but rejected because the spec tests OIDC specifically, *"assume large enterprise"* points to central identity, and it's more to build with weaker separation of concerns.

**Trade-off accepted — demo friction & external dependency:** the cost of Entra is a tenant + app registration, App-role assignment, and a token-acquisition flow to get test tokens (Postman / a client app / `az`), plus a dependency on Entra being reachable at demo time. Chosen anyway for the real enterprise OIDC story and Microsoft-stack fit. **Duende IdentityServer (local)** remains the fallback if Entra setup proves too costly for the timebox; ASP.NET Core Identity is the last resort ("demo-ability over enterprise-fidelity"). Note: because the API only *validates* tokens (it doesn't call downstream APIs), **no client secret is needed** — only non-secret tenant/client IDs — so nothing sensitive lands in config.

### 8b. Authorization (planned)

**Decision:** Role-based policies (`CanCapture`, `CanApprove`, `CanSoftDelete`), enforced **server-side** (not just UI), plus an authorization handler for workflow-transition rules (e.g. only a Manager may transition to Approved/Rejected).

**Why:** the UI can be bypassed; the API is the trust boundary. Only a Manager token succeeds on Approve; a Capturer gets 403.

---

## 9. Tooling / process

- **API docs:** Scalar (`Scalar.AspNetCore`) in place of Swagger UI.
- **Branching:** GitHub Flow — branch from up-to-date `main`, push early so CI runs, PR when ready.
- **Host / identity (target):** Azure App Service (PaaS); Microsoft Entra ID (fallback: Duende IdentityServer).

---

## Decisions deliberately deferred / out of scope
- Domain events + outbox for the read-store projection (stretch).
- Optimistic concurrency on updates (stretch).
- Separate class-library projects per layer (see §3).
- Building the Client Portal / OMS — mocked and diagrammed only.
- Messaging/service bus for this system — it is synchronous by design.
