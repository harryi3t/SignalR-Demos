using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MyGames.Startup))]
namespace MyGames
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
