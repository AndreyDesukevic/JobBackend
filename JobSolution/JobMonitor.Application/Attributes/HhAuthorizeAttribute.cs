using Microsoft.AspNetCore.Mvc;

namespace JobMonitor.Application.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HhAuthorizeAttribute : TypeFilterAttribute
{
    public HhAuthorizeAttribute() : base(typeof(HhAuthorizationFilter)) { }
}