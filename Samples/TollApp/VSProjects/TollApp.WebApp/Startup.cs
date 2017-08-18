using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TollApp.WebApp.Startup))]
namespace TollApp.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
