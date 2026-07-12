using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Orders;
using StayFlow.Application.Features.Orders.Commands;
using StayFlow.Application.Features.Orders.Queries;
using StayFlow.Domain.Orders;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class OrdersController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<ActionResult<PagedResult<OrderDto>>> ListOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] OrderStatus? status = null)
        => Ok(await Sender.Send(new GetOrdersQuery(page, pageSize, status)));

    [HttpPost]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
    {
        var id = await Sender.Send(command);
        return Ok(new { id });
    }

    [HttpPost("{id:guid}/prepare")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> MarkPreparing(Guid id)
    {
        await Sender.Send(new MarkOrderPreparingCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/deliver")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> MarkDelivered(Guid id)
    {
        await Sender.Send(new MarkOrderDeliveredCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await Sender.Send(new CancelOrderCommand(id));
        return NoContent();
    }
}
