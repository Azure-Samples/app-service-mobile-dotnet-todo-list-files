using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MobileAppsFileSampleService.Startup))]

namespace MobileAppsFileSampleService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}