using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExerciseApp.Startup))]
namespace ExerciseApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
