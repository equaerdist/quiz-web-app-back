using Serilog;

namespace quiz_web_app.Infrastructure.Extensions
{
    public static class HostExtensions
    {
        public static IHostBuilder ConfigureSerilog(this ConfigureHostBuilder host)
        {
            return host.UseSerilog((context, 
                                    services, 
                                    configuration) => configuration
                                                .ReadFrom.Configuration(context.Configuration)
                                                .WriteTo.Console());
        }
    }
}
