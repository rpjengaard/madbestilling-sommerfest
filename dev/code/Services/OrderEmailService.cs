using System.Net;
using Madbestilling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;

namespace Madbestilling.Services;

public class OrderEmailService : IOrderEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderEmailService(
        IEmailSender emailSender,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _emailSender = emailSender;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendUserReceiptAsync(OrderRecord order, IEnumerable<CartItem> items, string mobilePayBoxNr)
    {
        var subject = $"Din bestilling er modtaget – {order.ChildName}";
        var body = BuildUserReceiptHtml(order, items, mobilePayBoxNr);
        var message = new EmailMessage(null, order.Email, subject, body, true);
        await _emailSender.SendAsync(message, emailType: "OrderReceipt");
    }

    public async Task SendAdminNotificationAsync(OrderRecord order, IEnumerable<CartItem> items)
    {
        var receivers = _configuration
            .GetSection("Notifications:Receivers")
            .Get<string[]>() ?? Array.Empty<string>();

        if (receivers.Length == 0) return;

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request is not null
            ? $"{request.Scheme}://{request.Host}"
            : string.Empty;

        var subject = $"Ny bestilling: {order.ChildName} ({order.ChildClass})";
        var body = BuildAdminNotificationHtml(order, items, baseUrl);

        foreach (var receiver in receivers)
        {
            var message = new EmailMessage(null, receiver, subject, body, true);
            await _emailSender.SendAsync(message, emailType: "OrderNotification");
        }
    }

    private static string BuildUserReceiptHtml(OrderRecord order, IEnumerable<CartItem> items, string mobilePayBoxNr)
    {
        var rows = BuildItemRows(items);
        return $"""
        <!DOCTYPE html>
        <html lang="da">
        <head><meta charset="utf-8"><title>Kvittering</title></head>
        <body style="margin:0;padding:0;background:#eef0f4;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#eef0f4;padding:32px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;max-width:600px;">

                <tr>
                  <td style="background:#2d4b8a;padding:32px 40px;text-align:center;">
                    <h1 style="color:#ffffff;font-size:24px;margin:0;font-weight:900;">Sommerfest Madbestilling</h1>
                    <p style="color:#f4c200;margin:8px 0 0;font-size:14px;font-weight:700;">Bestillingskvittering</p>
                  </td>
                </tr>

                <tr>
                  <td style="padding:32px 40px 16px;">
                    <p style="color:#1a1a2e;font-size:15px;margin:0 0 8px;">Hej!</p>
                    <p style="color:#555;font-size:14px;line-height:1.6;margin:0;">
                      Vi har modtaget din bestilling til <strong>{WebUtility.HtmlEncode(order.ChildName)}</strong>
                      i klasse <strong>{WebUtility.HtmlEncode(order.ChildClass)}</strong>. Her er dit overblik:
                    </p>
                  </td>
                </tr>

                <tr>
                  <td style="padding:0 40px 24px;">
                    <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;">
                      <tr style="background:#eef0f4;">
                        <th style="text-align:left;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;letter-spacing:.06em;border-bottom:2px solid #2d4b8a;">Ret</th>
                        <th style="text-align:center;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;letter-spacing:.06em;border-bottom:2px solid #2d4b8a;">Antal</th>
                        <th style="text-align:right;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;letter-spacing:.06em;border-bottom:2px solid #2d4b8a;">Pris</th>
                      </tr>
                      {rows}
                      <tr>
                        <td colspan="2" style="padding:12px;text-align:right;font-weight:700;font-size:13px;color:#2d4b8a;border-top:2px solid #2d4b8a;">Total:</td>
                        <td style="padding:12px;text-align:right;font-weight:900;font-size:18px;color:#2d4b8a;border-top:2px solid #2d4b8a;">{order.TotalAmount.ToString("N2", new System.Globalization.CultureInfo("da-DK"))} kr.</td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <tr>
                  <td style="padding:0 40px 24px;">
                    <table width="100%" cellpadding="0" cellspacing="0">
                      <tr>
                        <td style="background:#f4c200;border-radius:12px;padding:20px 24px;">
                          <p style="margin:0 0 14px;font-weight:900;font-size:15px;color:#1a2d5a;">Betal med MobilePay Box</p>
                          <p style="margin:0 0 6px;font-size:13px;color:#1a2d5a;"><strong>Box nummer:</strong> {WebUtility.HtmlEncode(mobilePayBoxNr)}</p>
                          <p style="margin:0 0 6px;font-size:13px;color:#1a2d5a;"><strong>Beløb:</strong> {order.TotalAmount.ToString("N2", new System.Globalization.CultureInfo("da-DK"))} kr.</p>
                          <p style="margin:0;font-size:13px;color:#1a2d5a;"><strong>Besked:</strong> {WebUtility.HtmlEncode(order.ChildClass)} - {WebUtility.HtmlEncode(order.ChildName)} ({WebUtility.HtmlEncode(order.Phone)})</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <tr>
                  <td style="padding:0 40px 32px;">
                    <p style="font-size:13px;color:#666;line-height:1.6;margin:0;">
                      Husk at skrive den korrekte besked i MobilePay, så vi kan identificere din betaling ved afhentning.<br>
                      Vi glæder os til at se jer til sommerfesten!
                    </p>
                  </td>
                </tr>

                <tr>
                  <td style="background:#eef0f4;padding:18px 40px;text-align:center;">
                    <p style="color:#aaa;font-size:11px;margin:0;">Sommerfest Madbestilling &bull; Automatisk genereret mail</p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }

    private static string BuildAdminNotificationHtml(OrderRecord order, IEnumerable<CartItem> items, string baseUrl)
    {
        var rows = BuildItemRows(items);
        var backofficeLink = $"{baseUrl}/umbraco#/bestillinger?orderId={order.Id}";
        var createdAt = order.CreatedAt.ToString("dd/MM/yyyy HH:mm");

        return $"""
        <!DOCTYPE html>
        <html lang="da">
        <head><meta charset="utf-8"><title>Ny bestilling</title></head>
        <body style="margin:0;padding:0;background:#eef0f4;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#eef0f4;padding:32px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;max-width:600px;">

                <tr>
                  <td style="background:#2d4b8a;padding:32px 40px;">
                    <h1 style="color:#ffffff;font-size:22px;margin:0;font-weight:900;">Ny bestilling modtaget</h1>
                    <p style="color:#f4c200;margin:8px 0 0;font-size:13px;font-weight:700;">{createdAt}</p>
                  </td>
                </tr>

                <tr>
                  <td style="padding:28px 40px 16px;">
                    <h2 style="font-size:12px;text-transform:uppercase;letter-spacing:.08em;color:#2d4b8a;margin:0 0 14px;">Kundeoplysninger</h2>
                    <table cellpadding="0" cellspacing="0">
                      <tr><td style="padding:3px 16px 3px 0;font-size:13px;color:#888;width:80px;">Barn:</td><td style="padding:3px 0;font-size:13px;color:#1a1a2e;font-weight:700;">{WebUtility.HtmlEncode(order.ChildName)}</td></tr>
                      <tr><td style="padding:3px 16px 3px 0;font-size:13px;color:#888;">Klasse:</td><td style="padding:3px 0;font-size:13px;color:#1a1a2e;font-weight:700;">{WebUtility.HtmlEncode(order.ChildClass)}</td></tr>
                      <tr><td style="padding:3px 16px 3px 0;font-size:13px;color:#888;">Mobil:</td><td style="padding:3px 0;font-size:13px;color:#1a1a2e;">{WebUtility.HtmlEncode(order.Phone)}</td></tr>
                      <tr><td style="padding:3px 16px 3px 0;font-size:13px;color:#888;">E-mail:</td><td style="padding:3px 0;font-size:13px;color:#1a1a2e;">{WebUtility.HtmlEncode(order.Email)}</td></tr>
                    </table>
                  </td>
                </tr>

                <tr>
                  <td style="padding:0 40px 24px;">
                    <h2 style="font-size:12px;text-transform:uppercase;letter-spacing:.08em;color:#2d4b8a;margin:0 0 14px;">Bestilte retter</h2>
                    <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;">
                      <tr style="background:#eef0f4;">
                        <th style="text-align:left;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;border-bottom:2px solid #2d4b8a;">Ret</th>
                        <th style="text-align:center;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;border-bottom:2px solid #2d4b8a;">Antal</th>
                        <th style="text-align:right;padding:10px 12px;font-size:11px;color:#2d4b8a;text-transform:uppercase;border-bottom:2px solid #2d4b8a;">Pris</th>
                      </tr>
                      {rows}
                      <tr>
                        <td colspan="2" style="padding:12px;text-align:right;font-weight:700;font-size:13px;color:#2d4b8a;border-top:2px solid #2d4b8a;">Total:</td>
                        <td style="padding:12px;text-align:right;font-weight:900;font-size:18px;color:#2d4b8a;border-top:2px solid #2d4b8a;">{order.TotalAmount.ToString("N2", new System.Globalization.CultureInfo("da-DK"))} kr.</td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <tr>
                  <td style="padding:0 40px 32px;text-align:center;">
                    <a href="{backofficeLink}"
                       style="display:inline-block;background:#2d4b8a;color:#ffffff;text-decoration:none;font-weight:700;font-size:14px;padding:14px 32px;border-radius:100px;">
                      Se bestilling i backoffice
                    </a>
                  </td>
                </tr>

                <tr>
                  <td style="background:#eef0f4;padding:18px 40px;text-align:center;">
                    <p style="color:#aaa;font-size:11px;margin:0;">Intern notifikation &bull; Sommerfest Madbestilling</p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }

    private static string BuildItemRows(IEnumerable<CartItem> items)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var item in items)
        {
            sb.Append($"""
            <tr>
              <td style="padding:10px 12px;font-size:13px;color:#333;border-bottom:1px solid #eee;">{WebUtility.HtmlEncode(item.Name)}</td>
              <td style="padding:10px 12px;font-size:13px;color:#333;text-align:center;border-bottom:1px solid #eee;">{item.Qty}</td>
              <td style="padding:10px 12px;font-size:13px;color:#333;text-align:right;border-bottom:1px solid #eee;">{(item.Price * item.Qty).ToString("N2", new System.Globalization.CultureInfo("da-DK"))} kr.</td>
            </tr>
            """);
        }
        return sb.ToString();
    }
}
