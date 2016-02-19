using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SampleDynamo.Startup))]
namespace SampleDynamo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
