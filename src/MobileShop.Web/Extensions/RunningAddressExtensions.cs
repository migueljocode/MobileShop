namespace MobileShop.Web.Extensions;

/// <summary>Prints the running host and the addresses the server listens on, independent of the console log level.</summary>
public static class RunningAddressExtensions
{
    /// <summary>Writes one line to the console once the server has started.</summary>
    public static WebApplication PrintRunningAddress(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses ?? [];

            Console.WriteLine($"Running on host {Environment.MachineName} at {string.Join(", ", addresses)}");
        });

        return app;
    }
}
