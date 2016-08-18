using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(massive_moose.web.Startup))]
namespace massive_moose.web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
