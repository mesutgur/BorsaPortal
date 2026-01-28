using Microsoft.Owin;
using Owin;
using BorsaPortal.Web.Services;

[assembly: OwinStartupAttribute(typeof(BorsaPortal.Web.Startup))]
namespace BorsaPortal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Configure SignalR
            app.MapSignalR();

            // Start background services
            StockPriceUpdateService.Start();
        }
    }
}
