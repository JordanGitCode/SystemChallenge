using Microsoft.EntityFrameworkCore;
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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, "someone-else@x.com");

        Assert.Equal(OperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task SoftDelete_ThenGetById_ReturnsNotFound()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = TestDb.NewService(ctx);

        await sut.SoftDeleteAsync(product.Id, Manager);
        var result = await sut.GetByIdAsync(product.Id);

        Assert.Equal(OperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Reject_PendingVersion_ByDifferentUser_Succeeds_AndDoesNotSetApprovedPointer()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, Capturer);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(WorkflowStatus.Pending, version.Status);
    }

    [Fact]
    public async Task Submit_AlreadyPending_ReturnsInvalid()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        var result = await sut.SubmitVersionForReview(product.Id, version.Id, Capturer);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
    }

    [Fact]
    public async Task Restore_DeletedProduct_Succeeds_AndVisibleAgain()
    {
        using var ctx = TestDb.NewContext();
        var (product, _) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = TestDb.NewService(ctx);

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
        var sut = TestDb.NewService(ctx);

        var result = await sut.RestoreAsync(product.Id);

        Assert.Equal(OperationStatus.InvalidTransition, result.Status);
    }

    [Fact]
    public async Task Approve_ProjectsRowIntoReadStore()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);

        var row = await ctx.ProductReadModels.FindAsync(product.Id);
        Assert.NotNull(row);
        Assert.Equal(version.Name, row!.Name);
        Assert.Equal(version.Id, row.VersionId);
        Assert.Equal(Manager, row.ApprovedBy);
    }

    [Fact]
    public async Task SoftDelete_RemovesRowFromReadStore()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id },
            Manager);
        Assert.NotNull(await ctx.ProductReadModels.FindAsync(product.Id));

        await sut.SoftDeleteAsync(product.Id, Manager);

        Assert.Null(await ctx.ProductReadModels.FindAsync(product.Id));
    }

    [Fact]
    public async Task Approve_SecondVersion_UpsertsSingleReadRow()
    {
        using var ctx = TestDb.NewContext();
        var (product, v1) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = v1.Id }, Manager);

        var v2 = await TestDb.AddVersion(ctx, product.Id, 2, WorkflowStatus.Pending, Capturer, "V2 Name");
        await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = v2.Id }, Manager);

        var rowCount = await ctx.ProductReadModels.CountAsync(r => r.ProductId == product.Id);
        var row = await ctx.ProductReadModels.FindAsync(product.Id);

        Assert.Equal(1, rowCount);
        Assert.Equal("V2 Name", row!.Name);
        Assert.Equal(v2.Id, row.VersionId);
    }

    [Fact]
    public async Task Restore_ApprovedProduct_ReprojectsReadRow()
    {
        using var ctx = TestDb.NewContext();
        var (product, version) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        await sut.ApproveProductVersion(
            new WorkflowStatusChangeRequest { productId = product.Id, versionId = version.Id }, Manager);
        await sut.SoftDeleteAsync(product.Id, Manager);
        Assert.Null(await ctx.ProductReadModels.FindAsync(product.Id));

        await sut.RestoreAsync(product.Id);

        Assert.NotNull(await ctx.ProductReadModels.FindAsync(product.Id));
    }

    [Fact]
    public async Task GetPending_IncludesPendingVersions_AndExcludesOtherStatuses()
    {
        using var ctx = TestDb.NewContext();
        var (_, draft)    = await TestDb.SeedProduct(ctx, WorkflowStatus.Draft, Capturer);
        var (_, pending)  = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var (_, approved) = await TestDb.SeedProduct(ctx, WorkflowStatus.Approved, Capturer);
        var sut = TestDb.NewService(ctx);

        var ids = (await sut.GetPendingVersionsAsync()).Select(r => r.VersionId).ToHashSet();

        Assert.Contains(pending.Id, ids);
        Assert.DoesNotContain(draft.Id, ids);
        Assert.DoesNotContain(approved.Id, ids);
    }

    [Fact]
    public async Task GetPending_ExcludesVersionsOfSoftDeletedProducts()
    {
        using var ctx = TestDb.NewContext();
        var (product, pending) = await TestDb.SeedProduct(ctx, WorkflowStatus.Pending, Capturer);
        var sut = TestDb.NewService(ctx);

        // present in the queue before deletion
        var before = (await sut.GetPendingVersionsAsync()).Select(r => r.VersionId);
        Assert.Contains(pending.Id, before);

        await sut.SoftDeleteAsync(product.Id, Manager);

        // gone after the product is soft-deleted, even though the version is still Pending
        var after = (await sut.GetPendingVersionsAsync()).Select(r => r.VersionId);
        Assert.DoesNotContain(pending.Id, after);
    }
}