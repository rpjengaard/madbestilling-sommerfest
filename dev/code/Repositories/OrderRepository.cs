using Madbestilling.Models;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Madbestilling.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IScopeProvider _scopeProvider;

    public OrderRepository(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public int CreateOrder(OrderRecord order)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Insert(order);
        scope.Complete();
        return order.Id;
    }

    public OrderRecord? GetOrder(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.FirstOrDefault<OrderRecord>("WHERE id = @0", id);
    }

    public IEnumerable<OrderRecord> GetAllOrders()
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.Fetch<OrderRecord>("ORDER BY createdAt DESC");
    }

    public void UpdateStatus(int id, string status)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Execute(
            "UPDATE madbestilling_orders SET status = @0 WHERE id = @1",
            status, id);
        scope.Complete();
    }

    public void UpdateOrder(OrderRecord order)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Execute(
            "UPDATE madbestilling_orders SET childName=@0, childClass=@1, phone=@2, email=@3, status=@4 WHERE id=@5",
            order.ChildName, order.ChildClass, order.Phone, order.Email, order.Status, order.Id);
        scope.Complete();
    }

    public void DeleteOrder(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Execute("DELETE FROM madbestilling_orders WHERE id = @0", id);
        scope.Complete();
    }
}
