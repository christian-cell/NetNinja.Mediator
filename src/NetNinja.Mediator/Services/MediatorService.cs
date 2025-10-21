using System.Reflection;
using NetNinja.Mediator.Abstractions;

namespace NetNinja.Mediator.Services
{
    public class MediatorService : IMediatorService
    {
        private readonly IServiceProvider _serviceProvider;

        public MediatorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null) throw new InvalidOperationException("Handler not found");
            var method = handlerType.GetMethod("Handle");
            if (method == null) throw new InvalidOperationException("Handle method not found");
            try
            {
                var task = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
                if (task == null) throw new InvalidOperationException("Handler returned null");
                var result = await task;
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