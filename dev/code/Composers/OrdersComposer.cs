using Madbestilling.Repositories;
using Madbestilling.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Madbestilling.Composers;

public class OrdersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunOrdersMigration>();
    }
}
