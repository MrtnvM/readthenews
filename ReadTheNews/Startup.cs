using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReadTheNews.Startup))]
namespace ReadTheNews
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
