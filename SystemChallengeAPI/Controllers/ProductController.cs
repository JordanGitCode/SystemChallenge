using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("capture")]
        public async Task<IActionResult> CaptureProduct([FromBody] CreateProductRequest req)
        {
            var createdBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            var result = await _products.CreateAsync(req, createdBy!);
            return CreatedAtAction(nameof(CaptureProduct), new { id = result.Id }, result);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest req, Guid id)
        {
            var createdBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            var result = await _products.UpdateAsync(id, req, createdBy!);
            return Ok(result);
        }

        [HttpPost("submit/{productId}/{versionId}")]
        public async Task<IActionResult> SubmitVersionForReview(Guid productId, Guid versionId)
        {
            var submittedBy = User.FindFirstValue(ClaimTypes.Upn)
                            ?? User.Identity?.Name;

            if (submittedBy == null)
            {
                return BadRequest("An unexpected error has occurred");
            }

            var result = await _products.SubmitVersionForReview(productId, versionId, submittedBy);
            return Ok(result);
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
            return Ok(result);
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
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _products.GetByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }
    }
}
