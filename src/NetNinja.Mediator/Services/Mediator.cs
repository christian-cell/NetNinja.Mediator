using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NetNinja.Mediator.Abstractions;

namespace NetNinja.Mediator.Services
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mediator(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var token = _httpContextAccessor.HttpContext?.RequestAborted ?? cancellationToken;
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);
            
            if (handler == null) throw new InvalidOperationException("Handler not found");
            
            var method = handlerType.GetMethod("Handle");
            if (method == null) throw new InvalidOperationException("Handle method not found");
            
            try
            {
                Func<Task<TResponse>> pipeline = () =>
                {
                    var task = (Task<TResponse>)method.Invoke(handler, new object[] { request, token })!;
                    if (task == null) throw new InvalidOperationException("Handler returned null");

                    return task;
                };
                
                var behaviorInterface = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
                var behaviors = _serviceProvider.GetServices(behaviorInterface).ToList();

                foreach (var behavior in behaviors.AsEnumerable().Reverse())
                {
                    var next = pipeline;
                    var behaviorInstance = behavior;

                    var handleMethod = behaviorInterface.GetMethod("Handle");
                    if (handleMethod == null) throw new InvalidOperationException("Behavior Handle method not found on interface");

                    pipeline = () =>
                    {
                        var resultTask = (Task<TResponse>)handleMethod.Invoke(behaviorInstance, new object[] { request, token, next })!;
                        if (resultTask == null) throw new InvalidOperationException("Behavior returned null");
                        return resultTask;
                    };
                }


               var result = await pipeline();
               if (result == null) throw new InvalidOperationException("Handler returned null");
               return result;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }
    }
}