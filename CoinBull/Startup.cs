using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CoinBull.Startup))]
namespace CoinBull
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
