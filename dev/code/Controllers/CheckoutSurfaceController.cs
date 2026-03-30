using System.Text.Json;
using Madbestilling.Models;
using Madbestilling.Repositories;
using Madbestilling.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace Madbestilling.Controllers;

public class CheckoutSurfaceController : SurfaceController
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEmailService _emailService;
    private readonly ILogger<CheckoutSurfaceController> _logger;

    public CheckoutSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IOrderRepository orderRepository,
        IOrderEmailService emailService,
        ILogger<CheckoutSurfaceController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitOrder(CheckoutFormModel form)
    {
        if (!ModelState.IsValid)
            return CurrentUmbracoPage();

        if (!string.Equals(form.Email, form.EmailConfirm, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(form.EmailConfirm), "E-mailerne stemmer ikke overens.");
            return CurrentUmbracoPage();
        }

        List<CartItem> cartItems;
        try
        {
            cartItems = JsonSerializer.Deserialize<List<CartItem>>(
                form.CartJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CartItem>();
        }
        catch
        {
            ModelState.AddModelError(nameof(form.CartJson), "Ugyldig kurvdata.");
            return CurrentUmbracoPage();
        }

        if (cartItems.Count == 0)
        {
            ModelState.AddModelError(nameof(form.CartJson), "Kurven er tom.");
            return CurrentUmbracoPage();
        }

        var total = cartItems.Sum(i => i.Price * i.Qty);
        var mobilePayBoxNr = form.MobilePayBoxNumber.Trim();

        var order = new OrderRecord
        {
            ChildName   = form.ChildName.Trim(),
            ChildClass  = form.ChildClass.Trim(),
            Phone       = form.Phone.Trim(),
            Email       = form.Email.Trim(),
            CartJson    = form.CartJson,
            TotalAmount = total,
            Status      = "ny",
            CreatedAt   = DateTime.UtcNow,
        };

        var orderId = _orderRepository.CreateOrder(order);
        order.Id = orderId;

        try
        {
            await _emailService.SendUserReceiptAsync(order, cartItems, mobilePayBoxNr);
            await _emailService.SendAdminNotificationAsync(order, cartItems);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send emails for order {OrderId}", orderId);
        }

        TempData["Payment_BoxNr"]   = mobilePayBoxNr;
        TempData["Payment_Amount"]  = total.ToString("N2", new System.Globalization.CultureInfo("da-DK"));
        TempData["Payment_Message"] = $"{order.ChildClass} - {order.ChildName} ({order.Phone})";

        var returnUrl = string.IsNullOrWhiteSpace(form.ReturnUrl) ? "/" : form.ReturnUrl;
        return Redirect(returnUrl + "?success=true");
    }
}
