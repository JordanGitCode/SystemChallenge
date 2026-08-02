# Purpose

- The Product Management System is the single source of product information for the overall system.
- It owns a product's entire lifecycle from capture to approval
- It is the only system allowed to create, change, or approve product data

- The Client Portal and Order Management System calls the PMS, the PMS does not call the other systems.
- Meaning the PMS can be built, run, and demoed in isolation

## Capabilities

- Products can be captured, with multiple versions created later on by Capturer/Manager.
- Captured Products can be submitted for review by Capturer/Manager.
- Products can be approved or rejected ONLY by a manager. The system forbids approving/rejecting your own product version.
- A Product can be 'soft deleted' and restored ONLY by a manager.
- Approved product versions are automatically added to the data lake and removed when deleted.
- The purpose of the data lake is for fast reads to the Client Portal/Order Management System.

- It does not handle orders, carts, customers, or payments at all.
- Just product management

## Roles

- Capturer = create, read, update (submit for review).
- Manager = everything a Capturer can do, plus approve/reject and soft-delete/restore.

# Data

- The PMS is the sole owner/writer of all product data
- The Portal and OMS can only retreive product data through the PMS API.

## Entities

### Product

- Identity + Lifecycle metadata
- Id, IsDeleted/DeletedAt/DeletedBy, CreatedBy/At, CurrentApprovedVersionId

### ProductVersion

- An Immutable snapshot of the product's attributes at a workflow point
- VersionNumber, Name, Description, Price, Sku, Status, CreatedBy/At, DecidedBy/At/Reason

### ProductReadModel

- Denormalised, approved-only projection for fast consumer reads
- ProductId, Name, Description, Price, Sku, VersionId, ApprovedBy/At

## Design points

### Product vs ProductVersion

- A Product carries identity and lifecycle
- A ProductVersion carries the attributes
- A change creates a new ProductVersion with Draft status
- This means a full approval history and audit trail with non-destructive edits

### One live version

- Multiple versions can bear the Approved status over time
- Product.CurrentApprovedVersionId points to the single approved version that is "published"
- It is the source of truth for what consumers see.

### Immutability

- Once created, a version's attributes don't change
- Only workflow status and decision data can change
- This is what makes approval history trustworthy

### Soft-Delete

- The 'IsDeleted' flag means this Product and all its versions are filtered out of normal reads
- The data is never physically deleted

### CQRS (Command Query Responsibility Segregation)

- Using a different model to change data than the one used to read it.
- The write model (Product/ProductVersion) is the source of truth, while the read model (data lake) is a projection thereof
- Consumers only see the projection, not the write model

# API Interaction & Auth

## Catalog

- Client Portal & OMS
- Exposes approved, published products only
- Denormalised read model
- Read-only ProductReadDbContext
- Stable (unless a breaking change is wongly approved)

### Endpoints - catalog/approved products

- Get /calatog (List of approved products)
- Get /catalog/id (A single approved product)

## Internal management

- The PMS's own UI & internal users
- Exposes full capture to approve workflow
- Write model operations
- Write ApplicationDbContext
- No external dependents

### Endpoints

- POST /product/capture (Create draft i.e. v1)
- POST /product/update/id (New draft version)
- POST /product/submit (Draft - pending)
- POST /product/approve (Pending - approved)
- POST /product/reject (Pending - rejected)
- DELETE /product/id (Soft delete)
- POST /product/restore/id (Restore)
- GET /product & GET /product/id (Latest product versions)
- GET /product/pending (Pending approval)
- GET /me (Current user's identity & roles)

## Auth

- Microsoft Entra ID
- Roles: Capturer & Manager
- Policies
  - CanCapture (Capturer & Manager)
  - CanApprove (Manager)
  - CanSoftDelete (Manager)
- A manager cannot approve/reject a version they authored
- Rules enforced server-side

# Internal Architecture

## Request pipeline

Controller -> IProductService -> ApplicationDbContext + IProductProjector

- The Controller pulls the caller's identity from the token (ClaimTypes.Upn) and saves it to createdBy/ApprovedBy/RejectedBy
- IProductService handles task execution so the controllers stay 'thin', which depend on ApplicationDbContext & IProductProjector

## WorkflowStatus

### Create Draft

- CreateAsync()

### Edit / new Draft version

- UpdateAsync()
- The product exists
- Every edit = a new immutable product version created with the default Draft status
- The only mutations allowed are to Status and decision data (DecidedBy/At/Reason)
  - Trustworthy approval history

### Draft -> Pending

- SubmitVersionForReview()
- User who submits must own (have authored) the version
- Must be in draft state

### Pending -> Approved

- ApproveProductVersion()
- Approval cannot be done by the owner/author
- Must be in Pending state
- On approval, CurrentApprovedVersionId is changed to point to the newly approved version

### Pending -> Rejected

- RejectProductVersion()
- Rejection cannot be done by the owner/author
- Must be in Pending state

## CQRS Read model

- Not a separate store
- Model separation + denormalised read path

### Write

- Product + ProductVersion via ApplicationDbContext
- Normalised + Full history

### Read

- ProductReadModel
- Denormalised, approved-only
- One row per product
- Through ProductReadDbContext
- /catalog & /catalog/id

### Projector

- Keeps the read model in sync
- ProjectApprovedAsync -> upsert on approve (and on restore, if a live approved version exists)
- RemoveAsync -> delete the row on soft-delete

## Error handling

- The Service returns OperationResult<T> Success/NotFound/Forbidden/InvalidTransition
- ToActionResult in the controller maps that to HttpResponses (404, 403, 409, 200, 201, 204)
