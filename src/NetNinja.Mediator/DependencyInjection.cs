using Microsoft.Extensions.DependencyInjection;
using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Services;
using System.Reflection;
using System.Linq;

namespace NetNinja.Mediator
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNetNinjaMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddTransient<IMediatorService, MediatorService>();

            var handlerInterface = typeof(IRequestHandler<,>);
            var handlerTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterface)
                .ToList();

            foreach (var handler in handlerTypes)
            {
                services.AddTransient(handler.Interface, handler.Type);
            }

            return services;
        }
    }
}