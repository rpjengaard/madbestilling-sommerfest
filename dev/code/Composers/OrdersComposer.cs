using Madbestilling.Repositories;
using Madbestilling.Services;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Madbestilling.Composers;

public class OrdersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddHttpClient<ResendClient>();
        builder.Services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = builder.Config["Resend:ApiToken"]!;
        });
        builder.Services.AddTransient<IResend, ResendClient>();

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunOrdersMigration>();
    }
}
