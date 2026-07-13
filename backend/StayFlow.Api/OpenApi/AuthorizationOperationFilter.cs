using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StayFlow.Api.OpenApi;

/// <summary>Overrides document-level OAuth for endpoints that do not require authorization.</summary>
public sealed class AuthorizationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var controller = method.DeclaringType;
        var allowsAnonymous = method.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
            || controller?.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) == true;
        var requiresAuthorization = method.IsDefined(typeof(AuthorizeAttribute), inherit: true)
            || controller?.IsDefined(typeof(AuthorizeAttribute), inherit: true) == true;

        if (allowsAnonymous || !requiresAuthorization)
        {
            // An empty operation-level requirement explicitly overrides document-level OAuth.
            operation.Security = [];
        }
    }
}
