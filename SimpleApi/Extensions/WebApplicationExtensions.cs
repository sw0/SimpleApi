using System.Net;

namespace SimpleApi.Extensions;


public static class WebApplicationExtensions
{
    public static void MapHome(this IEndpointRouteBuilder app)
    {
        app.MapGet("~/", async (context) =>
        {
            var logger = context.RequestServices.GetService<ILogger<Program>>();

            logger?.LogInformation("Path / got accessed.");

            await context.Response.WriteAsync("welcome simpleApi test page. Please go to ~/swagger to test redis connection by post/get and post/set API");
        });
    }

    public static void MapApiPing(this IEndpointRouteBuilder endpoints, string path = "~/api/ping")
    {
        endpoints.MapPing(path);
    }

    public static void MapPing(this IEndpointRouteBuilder app, string path = "~/ping")
    {
        app.MapGet(path, async (context) =>
        {
            var env = context.RequestServices.GetService<IWebHostEnvironment>();
            if (env != null)
            {
                context.Response.Headers.Add("X-Environment", env!.EnvironmentName);
            }

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

            await context.Response.WriteAsJsonAsync(new
            {
                netCoreVersion = Environment.Version.ToString(),
                ipAddresses = ipEntry.AddressList.Select(x => x.ToString()).ToArray()
            });
        });
    }
}
