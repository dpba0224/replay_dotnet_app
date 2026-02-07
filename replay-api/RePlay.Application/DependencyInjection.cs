using Microsoft.Extensions.DependencyInjection;

namespace RePlay.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services will be registered here as they are implemented
        // services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IToyService, ToyService>();
        // services.AddScoped<ITradeService, TradeService>();
        // etc.

        return services;
    }
}
