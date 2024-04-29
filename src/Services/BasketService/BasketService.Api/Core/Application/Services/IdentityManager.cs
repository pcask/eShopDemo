using System.Security.Claims;

namespace BasketService.Api.Core.Application.Services;

public class IdentityManager(IHttpContextAccessor httpContextAccessor) : IIdentityService
{
    public string GetUserName() // Get current user's userName
    {
        return httpContextAccessor.HttpContext!.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
    }
}
