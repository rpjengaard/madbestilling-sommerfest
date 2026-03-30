using Madbestilling.Models;

namespace Madbestilling.Repositories;

public interface IOrderRepository
{
    int CreateOrder(OrderRecord order);
    OrderRecord? GetOrder(int id);
    IEnumerable<OrderRecord> GetAllOrders();
    void UpdateStatus(int id, string status);
    void UpdateOrder(OrderRecord order);
    void DeleteOrder(int id);
}
