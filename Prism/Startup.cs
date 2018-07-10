using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Prism.Startup))]
namespace Prism
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
