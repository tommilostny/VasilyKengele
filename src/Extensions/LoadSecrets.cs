namespace VasilyKengele.Extensions;

public static class LoadSecrets
{
    public static VKConfiguration AddVKSecrets(this IServiceCollection services)
    {
        // Loading secrets work-around using a second temporary configuration.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<VKConfiguration>()
            .Build();

        var serviceProvider = new ServiceCollection()
            .Configure<VKConfiguration>(configuration.GetSection(nameof(VKConfiguration)))
            .AddOptions()
            .BuildServiceProvider();

        var config = serviceProvider.GetService<IOptions<VKConfiguration>>()?.Value
            ?? throw new Exception("Secrets not loaded...");

        services.AddSingleton(config);
        return config;
    }
}
