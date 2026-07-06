using SystemChallengeAPI.Domain;
using SystemChallengeAPI.DTOs;
using SystemChallengeAPI.Services;
using Xunit;

public class ProductWorkflowTests
{
    private const string Capturer = "capturer@x.com";
    private const string Manager = "manager@x.com";

    [Fact]
    public async Task Approve_DraftVersion_ReturnsInvalid_AndLeavesStatusUnchanged()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Draft, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
        Assert.Equal(WorkflowStatus.Draft, version.Status);
        Assert.Null(product.CurrentApprovedVersionId);
    }

    [Fact]
    public async Task Approve_PendingVersion_ByDifferentUser_Succeeds_AndSetsApprovedPointer()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id, DecisionReason = "ok" },
            Manager);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(WorkflowStatus.Approved, version.Status);
        Assert.Equal(version.Id, product.CurrentApprovedVersionId);
        Assert.Equal(Manager, version.DecidedBy);
        Assert.NotNull(version.DecidedAt);
    }

    [Fact]
    public async Task Approve_OwnVersion_ReturnsForbidden()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Manager);
        var sut = new ProductService(ctx);

        var result = await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);

        Assert.Equal(OperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task Submit_ByNonOwner_ReturnsForbidden()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Draft, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, "someone-else@x.com");

        Assert.Equal(OperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task SoftDelete_ThenGetById_ReturnsNotFound()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = new ProductService(ctx);

        await sut.SoftDeleteAsync(product.Id, Manager);
        var result = await sut.GetByIdAsync(product.Id);

        Assert.Equal(OperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Reject_PendingVersion_ByDifferentUser_Succeeds_AndDoesNotSetApprovedPointer()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.RejectProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id, DecisionReason = "bad sku" },
            Manager);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(WorkflowStatus.Rejected, version.Status);
        Assert.Null(product.CurrentApprovedVersionId);
        Assert.Equal(Manager, version.DecidedBy);
        Assert.Equal("bad sku", version.DecisionReason);
    }

    [Fact]
    public async Task Reject_OwnVersion_ReturnsForbidden()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Manager);
        var sut = new ProductService(ctx);

        var result = await sut.RejectProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);

        Assert.Equal(OperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task Reject_NonPendingVersion_ReturnsInvalid()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Draft, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.RejectProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
        Assert.Equal(WorkflowStatus.Draft, version.Status);
    }

    [Fact]
    public async Task Approve_MissingProduct_ReturnsNotFound()
    {
        using var ctx = TestDb.NewContext();
        var sut = new ProductService(ctx);

        var result = await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = Guid.NewGuid(), versionId = Guid.NewGuid() },
            Manager);

        Assert.Equal(OperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Approve_MissingVersion_ReturnsNotFound()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = Guid.NewGuid() },  // real product, bogus version
            Manager);

        Assert.Equal(OperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Submit_DraftByOwner_Succeeds_AndSetsPending()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Draft, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, Capturer);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(WorkflowStatus.Pending, version.Status);
    }

    [Fact]
    public async Task Submit_AlreadyPending_ReturnsInvalid()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, Capturer);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
    }

    [Fact]
    public async Task Restore_DeletedProduct_Succeeds_AndVisibleAgain()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = new ProductService(ctx);

        await sut.SoftDeleteAsync(product.Id, Manager);
        var restore = await sut.RestoreAsync(product.Id);
        var get = await sut.GetByIdAsync(product.Id);

        Assert.Equal(OperationStatus.Success, restore.Status);
        Assert.Equal(OperationStatus.Success, get.Status);   // visible through the filter again
    }

    [Fact]
    public async Task Restore_NonDeletedProduct_ReturnsInvalid()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = new ProductService(ctx);

        var result = await sut.RestoreAsync(product.Id);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
    }
}