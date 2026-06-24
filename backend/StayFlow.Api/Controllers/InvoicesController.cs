using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Billing;
using StayFlow.Application.Features.Billing.Commands;
using StayFlow.Application.Features.Billing.Queries;
using StayFlow.Domain.Billing;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class InvoicesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.BillingRead)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] InvoiceStatus? status = null)
        => Ok(await Sender.Send(new GetInvoicesQuery(page, pageSize, status)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.BillingRead)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id)
        => Ok(await Sender.Send(new GetInvoiceByIdQuery(id)));

    [HttpPost]
    [Authorize(Policy = Permissions.BillingManage)]
    public async Task<ActionResult<InvoiceDto>> Generate([FromBody] GenerateInvoiceCommand command)
    {
        var invoice = await Sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Policy = Permissions.BillingManage)]
    public async Task<IActionResult> Pay(Guid id)
    {
        await Sender.Send(new MarkInvoicePaidCommand(id));
        return NoContent();
    }
}
