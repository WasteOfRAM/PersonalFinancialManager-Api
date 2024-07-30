namespace PersonalFinancialManager.Application;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.ToString().EndsWith("Service")).ToList();

        foreach (var type in types)
        {
            var @interface = type.GetInterface("I" + type.Name);

            if (@interface == null)
                continue;

            services.AddScoped(@interface, type);
        }

        return services;
    }
}
