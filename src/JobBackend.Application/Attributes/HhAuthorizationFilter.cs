using JobBackend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace JobBackend.Application.Attributes;

public class HhAuthorizationFilter : IAuthorizationFilter
{
    private readonly IUserService _userService;

    public HhAuthorizationFilter(IUserService userService)
    {
        _userService = userService;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hhId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(hhId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var dbUser = _userService.GetByHhIdAsync(hhId, CancellationToken.None).Result;

        if (dbUser == null)
        {
            context.Result = new ForbidResult();
        }
    }
}