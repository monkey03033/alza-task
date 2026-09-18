using Alza.Contracts;
using Alza.Delivery.ApplicationLayer.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Alza.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        await _orderService.HandleOrder(request, CancellationToken.None);

        return NoContent();
    }
}