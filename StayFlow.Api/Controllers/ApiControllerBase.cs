using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StayFlow.Api.Controllers;

/// <summary>Base for API controllers: lazily resolves the MediatR sender used to dispatch requests.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
