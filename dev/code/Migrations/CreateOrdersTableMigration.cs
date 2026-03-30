using Madbestilling.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Madbestilling.Migrations;

public class CreateOrdersTableMigration : MigrationBase
{
    public CreateOrdersTableMigration(IMigrationContext context) : base(context) { }

    protected override void Migrate()
    {
        if (!TableExists("madbestilling_orders"))
        {
            Create.Table<OrderRecord>().Do();
        }
    }
}
