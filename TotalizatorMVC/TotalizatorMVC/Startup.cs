using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TotalizatorMVC.Startup))]
namespace TotalizatorMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
