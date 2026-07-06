using SystemChallengeAPI.Domain;
using SystemChallengeAPI.DTOs;

namespace SystemChallengeAPI.Services
{
    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(CreateProductRequest req, string createdBy);
        Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest req, string createdBy);
        Task<ProductResponse> GetByIdAsync(Guid id);
        Task<ProductResponse> SubmitVersionForReview(Guid productId, Guid versionId, string submittedBy);
        Task<ProductResponse> ApproveProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string approvedBy);
        Task<ProductResponse> RejectProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string rejectedBy);
    }
}
