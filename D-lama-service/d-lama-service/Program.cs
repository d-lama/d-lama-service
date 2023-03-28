using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace d_lama_service
{
    /// <summary>
    /// Program class.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Main function.
        /// </summary>
        /// <param name="args"> Ignored string parameters. </param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var startup = new Startup(builder.Configuration, builder.Environment);
            startup.ConfigureServices(builder.Services); // calling ConfigureServices method
            var app = builder.Build();
            startup.Configure(app); // calling Configure method
            app.Run();
        }
    }
}
