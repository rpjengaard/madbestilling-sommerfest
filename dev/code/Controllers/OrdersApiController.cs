using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
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

    // [CHANGE: replace status values, add note field]  Related: Models/OrderRecord.cs, App_Plugins/orders/orders-dashboard.js
    private static readonly string[] ValidStatuses = ["ny", "order-betalt", "problem"];

    [HttpPatch("UpdateStatus/{id:int}")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
            return BadRequest("Ugyldig status.");

        if (_orderRepository.GetOrder(id) is null)
            return NotFound();

        _orderRepository.UpdateStatus(id, request.Status);
        return Ok();
    }

    [HttpPut("UpdateOrder/{id:int}")]
    public IActionResult UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
            return BadRequest("Ugyldig status.");

        var order = _orderRepository.GetOrder(id);
        if (order is null) return NotFound();

        order.ChildName  = request.ChildName.Trim();
        order.ChildClass = request.ChildClass.Trim();
        order.Phone      = request.Phone.Trim();
        order.Email      = request.Email.Trim();
        order.Status     = request.Status;
        order.Note       = request.Status == "problem" ? request.Note?.Trim() : null;

        _orderRepository.UpdateOrder(order);
        return Ok(order);
    }

    // [CHANGE: Excel export endpoint]  Related: App_Plugins/orders/orders-dashboard.js, code.csproj
    [HttpGet("ExportOrders")]
    public IActionResult ExportOrders()
    {
        var orders = _orderRepository.GetAllOrders().ToList();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Bestillinger");

        string[] headers = ["#", "Barn", "Klasse", "Mobil", "E-mail", "Total (kr.)", "Status", "Note", "Tidspunkt", "Retter"];
        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2d4b8a");
            sheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        var row = 2;
        foreach (var order in orders)
        {
            sheet.Cell(row, 1).Value = order.Id;
            sheet.Cell(row, 2).Value = order.ChildName;
            sheet.Cell(row, 3).Value = order.ChildClass;
            sheet.Cell(row, 4).Value = order.Phone;
            sheet.Cell(row, 5).Value = order.Email;
            sheet.Cell(row, 6).Value = (double)order.TotalAmount;
            sheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 7).Value = order.Status;
            sheet.Cell(row, 8).Value = order.Note ?? string.Empty;
            sheet.Cell(row, 9).Value = order.CreatedAt.ToLocalTime();
            sheet.Cell(row, 9).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";
            sheet.Cell(row, 10).Value = FormatItems(order.CartJson);
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"bestillinger-{DateTime.Now:yyyy-MM-dd-HHmm}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static string FormatItems(string cartJson)
    {
        try
        {
            var items = JsonSerializer.Deserialize<List<CartItem>>(cartJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return string.Join("; ", items.Select(i =>
                $"{i.Qty}x {i.Name} ({(i.Price * i.Qty).ToString("F2", CultureInfo.InvariantCulture)} kr.)"));
        }
        catch
        {
            return cartJson;
        }
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
public record UpdateOrderRequest(string ChildName, string ChildClass, string Phone, string Email, string Status, string? Note);
