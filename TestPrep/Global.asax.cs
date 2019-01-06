using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GemBox.Spreadsheet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestPrep.Services;

namespace TestPrep
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            var jSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            jsonFormatter.SerializerSettings = jSettings;

            //Start Background Services
            ServicesScheduler.Start();

            //Gembox Initializer
            SpreadsheetInfo.SetLicense("EJ35-H1BP-2SND-7Q0R");
        }
    }
}
