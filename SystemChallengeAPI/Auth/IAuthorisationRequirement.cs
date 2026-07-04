using Microsoft.AspNetCore.Authorization;
using SystemChallengeAPI.Domain;

namespace SystemChallengeAPI.Auth
{
    public class ApprovalRequirement : IAuthorizationRequirement { }

    public class ApprovalHandler : AuthorizationHandler<ApprovalRequirement, ProductVersion>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApprovalRequirement requirement,
            ProductVersion resource)
        {
            // TODO (separation of duties): succeed only if Manager/approver != author
            // For now, succeed if Manager
            if (context.User.IsInRole(Roles.Manager))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

}
