using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppERP1.Startup))]
namespace WebAppERP1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
