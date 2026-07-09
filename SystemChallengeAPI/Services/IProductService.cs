using SystemChallengeAPI.Domain;
using SystemChallengeAPI.DTOs;

namespace SystemChallengeAPI.Services
{
    public interface IProductService
    {
        Task<OperationResult<ProductResponse>> CreateAsync(CreateProductRequest req, string createdBy);
        Task<OperationResult<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest req, string createdBy);
        Task<OperationResult<ProductResponse>> GetByIdAsync(Guid id);
        Task<List<ProductResponse>> GetAllProductsAsync();
        Task<OperationResult<ProductResponse>> SubmitVersionForReview(Guid productId, Guid versionId, string submittedBy);
        Task<OperationResult<ProductResponse>> ApproveProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string approvedBy);
        Task<OperationResult<ProductResponse>> RejectProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string rejectedBy);
        Task<OperationResult<ProductResponse>> SoftDeleteAsync(Guid id, string deletedBy);
        Task<OperationResult<ProductResponse>> RestoreAsync(Guid id);
        Task<List<PendingVersionResponse>> GetPendingVersionsAsync();
    }
}
