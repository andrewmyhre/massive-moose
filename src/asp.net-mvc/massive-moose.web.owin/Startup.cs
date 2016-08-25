using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(massive_moose.web.owin.Startup))]
namespace massive_moose.web.owin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateDefaultWall(app);
        }
    }
}
