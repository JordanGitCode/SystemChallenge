using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemChallengeAPI.Auth;

namespace SystemChallengeAPI.API
{

    [ApiController]
    [Route("me")]
    [Authorize]
    public class MeController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            var name = User.Identity?.Name
                ?? User.FindFirstValue("name")
                ?? User.FindFirstValue("preferred_username");

            var identity = (ClaimsIdentity)User.Identity!;
            var roles = User.FindAll(identity.RoleClaimType).Select(c => c.Value).ToArray();

            return Ok(new { name, roles, claims = User.Claims.Select(c => new { c.Type, c.Value }) });
        }

        // Temp endpoint to test the CanApprove policy
        [Authorize(Policy = Policies.CanApprove)]
        [HttpGet("approve-test")]
        public IActionResult ApproveTest() => Ok("manager-only reached");

    }
}
