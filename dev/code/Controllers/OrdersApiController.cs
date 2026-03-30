using Madbestilling.Models;
using Madbestilling.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Madbestilling.Controllers;

[ApiController]
[Route("umbraco/api/madbestilling/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;

    public OrdersApiController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet("GetAllOrders")]
    public IActionResult GetAllOrders()
    {
        var orders = _orderRepository.GetAllOrders();
        return Ok(orders);
    }

    [HttpGet("GetOrder/{id:int}")]
    public IActionResult GetOrder(int id)
    {
        var order = _orderRepository.GetOrder(id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPatch("UpdateStatus/{id:int}")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        string[] valid = ["ny", "betaling-godkendt", "klar-til-afhentning"];
        if (!valid.Contains(request.Status))
            return BadRequest("Ugyldig status.");

        if (_orderRepository.GetOrder(id) is null)
            return NotFound();

        _orderRepository.UpdateStatus(id, request.Status);
        return Ok();
    }

    [HttpPut("UpdateOrder/{id:int}")]
    public IActionResult UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var order = _orderRepository.GetOrder(id);
        if (order is null) return NotFound();

        order.ChildName  = request.ChildName.Trim();
        order.ChildClass = request.ChildClass.Trim();
        order.Phone      = request.Phone.Trim();
        order.Email      = request.Email.Trim();
        order.Status     = request.Status;

        _orderRepository.UpdateOrder(order);
        return Ok(order);
    }

    [HttpDelete("DeleteOrder/{id:int}")]
    public IActionResult DeleteOrder(int id)
    {
        if (_orderRepository.GetOrder(id) is null)
            return NotFound();

        _orderRepository.DeleteOrder(id);
        return Ok();
    }
}

public record UpdateStatusRequest(string Status);
public record UpdateOrderRequest(string ChildName, string ChildClass, string Phone, string Email, string Status);
