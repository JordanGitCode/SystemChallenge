using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SystemChallengeAPI.API
{

    [ApiController]
    [Route("me")]
    [Authorize]
    public class MeController : Controller
    {

        [HttpGet]
        public IActionResult Get()
        {
            var name = User.Identity?.Name
                ?? User.FindFirstValue("name")
                ?? User.FindFirstValue("preferred_username");

            var roles = User.FindAll("roles").Select(c => c.Value).ToArray();

            return Ok(new { name, roles, claims = User.Claims.Select(c => new { c.Type, c.Value }) });
        }
    }
}
