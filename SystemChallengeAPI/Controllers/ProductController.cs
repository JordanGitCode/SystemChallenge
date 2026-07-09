using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemChallengeAPI.Auth;
using SystemChallengeAPI.Domain;
using SystemChallengeAPI.DTOs;
using SystemChallengeAPI.Services;

namespace SystemChallengeAPI.Controllers
{

    [ApiController]
    [Route("product")]
    [Authorize(Policy = Policies.CanCapture)]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _products;

        public ProductController(IProductService products)
        {
            _products = products;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var result = await _products.GetByIdAsync(id);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
        {
            var result = await _products.GetAllProductsAsync();

            return Ok(result);
        }

        [HttpPost("capture")]
        public async Task<IActionResult> CaptureProduct([FromBody] CreateProductRequest req)
        {
            var createdBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (createdBy == null)
                return BadRequest("An unexpected error occurred");

            var result = await _products.CreateAsync(req, createdBy!);
            return CreatedAtAction(nameof(CaptureProduct), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest req, Guid id)
        {
            var createdBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (createdBy == null)
                return BadRequest("An unexpected error occurred");

            var result = await _products.UpdateAsync(id, req, createdBy!);
            return ToActionResult(result);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitVersionForReview([FromBody] WorkflowStatusChangeRequest workflowStatusChangeRequest)
        {
            var submittedBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (submittedBy == null)
            {
                return BadRequest("An unexpected error has occurred");
            }

            var result = await _products.SubmitVersionForReview(workflowStatusChangeRequest.productId, workflowStatusChangeRequest.versionId, submittedBy);
            return ToActionResult(result);
        }

        [Authorize(Policy = Policies.CanApprove)]
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveProductVersion([FromBody] WorkflowStatusChangeRequest workflowStatusChangeRequest)
        {

            var approvedBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (approvedBy == null)
            {
                return BadRequest("An unexpected error has occurred");
            }

            var result = await _products.ApproveProductVersion(workflowStatusChangeRequest, approvedBy);
            return ToActionResult(result);
        }

        [Authorize(Policy = Policies.CanApprove)]
        [HttpPost("reject")]
        public async Task<IActionResult> RejectProductVersion([FromBody] WorkflowStatusChangeRequest workflowStatusChangeRequest)
        {

            var rejectedBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (rejectedBy == null)
            {
                return BadRequest("An unexpected error has occurred");
            }

            var result = await _products.RejectProductVersion(workflowStatusChangeRequest, rejectedBy);
            return ToActionResult(result);
        }

        [Authorize(Policy = Policies.CanSoftDelete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDeleteProduct(Guid id)
        {
            var deletedBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (deletedBy == null)
                return BadRequest("An unexpected error has occurred");

            var result = await _products.SoftDeleteAsync(id, deletedBy);

            if (result.Status == OperationStatus.Success)
                return NoContent();

            return ToActionResult(result);
        }

        [Authorize(Policy = Policies.CanSoftDelete)]
        [HttpPost("restore/{id:guid}")]
        public async Task<IActionResult> RestoreProduct(Guid id)
        {
            var result = await _products.RestoreAsync(id);
            return ToActionResult(result);
        }

        [Authorize(Policy = Policies.CanApprove)]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PendingVersionResponse>>> GetPendingVersions()
        {
            var result = await _products.GetPendingVersionsAsync();
            return Ok(result);
        }

        private IActionResult ToActionResult(OperationResult<ProductResponse> result)
        {

            switch (result.Status)
            {
                case OperationStatus.Success:
                    return Ok(result.Value);
                case OperationStatus.NotFound:
                    return Problem(detail: result.Error, statusCode: StatusCodes.Status404NotFound);
                case OperationStatus.Forbidden:
                    return Problem(detail: result.Error, statusCode: StatusCodes.Status403Forbidden);
                case OperationStatus.InvalidTransition:
                    return Problem(detail: result.Error, statusCode: StatusCodes.Status409Conflict);
                default:
                    return Problem();

            }
                        
        }

    }
}
