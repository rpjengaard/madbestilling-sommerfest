using Umbraco.Cms.Infrastructure.Migrations;

namespace Madbestilling.Migrations;

public class OrdersMigrationPlan : MigrationPlan
{
    public OrdersMigrationPlan() : base("Madbestilling.Orders")
    {
        From(string.Empty)
            .To<CreateOrdersTableMigration>("orders-table-v1")
            .To<AddNoteColumnMigration>("orders-table-v2-note");
    }
}
