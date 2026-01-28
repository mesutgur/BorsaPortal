using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Migrations;
using BorsaPortal.Web.Models;

namespace BorsaPortal.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Enable automatic database migrations
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BorsaPortalDbContext, Configuration>());

            // Force database initialization
            using (var context = new BorsaPortalDbContext())
            {
                context.Database.Initialize(force: false);
            }

            // Register custom model binder for decimal to support both comma and dot separators
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Set culture for each request to ensure consistent decimal handling
            var culture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
