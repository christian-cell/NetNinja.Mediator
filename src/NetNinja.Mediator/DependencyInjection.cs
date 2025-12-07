using Microsoft.Extensions.DependencyInjection;
using NetNinja.Mediator.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace NetNinja.Mediator
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNetNinjaMediator(this IServiceCollection services, params Assembly[] assemblies)
            => AddNetNinjaMediator(services, registerValidators: false, registerDefaultValidationBehavior: false, assemblies: assemblies);

        public static IServiceCollection AddNetNinjaMediator(this IServiceCollection services, bool registerValidators, bool registerDefaultValidationBehavior, params Assembly[] assemblies)
        {
            services.AddTransient<IMediator, Services.Mediator>();

            var handlerInterface = typeof(IRequestHandler<,>);
            var pipelineInterface = typeof(IPipelineBehavior<,>);
            var validatorInterface = Type.GetType("FluentValidation.IValidator`1, FluentValidation")?.GetGenericTypeDefinition();

            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => !t.IsNested)
                .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                .ToList();

            // Handlers
            var handlerTypes = types
                .SelectMany(t => t.GetInterfaces().Select(i => new { Type = t, Interface = i }))
                .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterface)
                .ToList();

            foreach (var h in handlerTypes)
                services.AddTransient(h.Interface, h.Type);

            // Pipelines (closed and open generic)
            var registeredOpenPipelines = new HashSet<Type>();
            foreach (var t in types)
            {
                var pipelineIfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == pipelineInterface)
                    .ToList();

                if (!pipelineIfaces.Any()) continue;

                if (t.IsGenericTypeDefinition)
                {
                    if (registeredOpenPipelines.Add(t))
                        services.AddTransient(typeof(IPipelineBehavior<,>), t);
                }
                else
                {
                    foreach (var iface in pipelineIfaces)
                        services.AddTransient(iface, t);
                }
            }

            if (registerDefaultValidationBehavior)
            {
                var valBehaviorType = types.FirstOrDefault(t => t.IsGenericTypeDefinition && t.Name == "ValidationBehavior`2");
                if (valBehaviorType != null)
                {
                    services.AddTransient(typeof(IPipelineBehavior<,>), valBehaviorType);
                }
            }

            if (validatorInterface != null)
            {
                var validatorTypes = types
                    .SelectMany(t => t.GetInterfaces().Select(i => new { Type = t, Interface = i }))
                    .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition().FullName == validatorInterface.FullName)
                    .ToList();

                foreach (var v in validatorTypes)
                    services.AddTransient(v.Interface, v.Type);
            }

            if (registerValidators)
                TryInvokeFluentValidationAddValidatorsFromAssemblies(services, assemblies);

            return services;
        }

        private static void TryInvokeFluentValidationAddValidatorsFromAssemblies(IServiceCollection services, Assembly[] assemblies)
        {
            try
            {
                var fvAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(a => a.GetName().Name.Equals("FluentValidation", StringComparison.OrdinalIgnoreCase));
                if (fvAssembly == null) return;

                var extType = fvAssembly.GetTypes()
                              .FirstOrDefault(t => t.IsSealed && t.IsAbstract && t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                  .Any(m => m.Name == "AddValidatorsFromAssemblies"));

                if (extType == null) return;

                var method = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                    .FirstOrDefault(m => m.Name == "AddValidatorsFromAssemblies"
                                                      && m.GetParameters().Length >= 2);

                if (method == null) return;

                method.Invoke(null, new object[] { services, assemblies });
            }
            catch
            {
                // silencioso: ya registramos validators manualmente si estaban presentes
            }
        }
    }
}
