using Madbestilling.Models;

namespace Madbestilling.Services;

public interface IOrderEmailService
{
    Task SendUserReceiptAsync(OrderRecord order, IEnumerable<CartItem> items, string mobilePayBoxNr);
    Task SendAdminNotificationAsync(OrderRecord order, IEnumerable<CartItem> items);
}
