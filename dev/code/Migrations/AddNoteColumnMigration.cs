using Umbraco.Cms.Infrastructure.Migrations;

namespace Madbestilling.Migrations;

// [CHANGE: add note field for "Problem" status]  Related: Models/OrderRecord.cs, Migrations/OrdersMigrationPlan.cs, Controllers/OrdersApiController.cs, App_Plugins/orders/orders-dashboard.js
public class AddNoteColumnMigration : MigrationBase
{
    public AddNoteColumnMigration(IMigrationContext context) : base(context) { }

    protected override void Migrate()
    {
        if (TableExists("madbestilling_orders") && !ColumnExists("madbestilling_orders", "note"))
        {
            Alter.Table("madbestilling_orders")
                .AddColumn("note").AsCustom("NVARCHAR(MAX)").Nullable()
                .Do();
        }
    }
}
