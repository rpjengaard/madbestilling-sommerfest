using Umbraco.Cms.Infrastructure.Migrations;

namespace Madbestilling.Migrations;

public class OrdersMigrationPlan : MigrationPlan
{
    public OrdersMigrationPlan() : base("Madbestilling.Orders")
    {
        From(string.Empty)
            .To<CreateOrdersTableMigration>("orders-table-v1");
    }
}
