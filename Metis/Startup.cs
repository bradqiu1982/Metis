using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Metis.Startup))]
namespace Metis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
