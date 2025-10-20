using NetNinja.Mediator.Abstractions;

namespace NetNinja.Mediator.Services
{
    public class MediatorService: IMediatorService
    {
        private readonly IServiceProvider _serviceProvider;

        public MediatorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string Greet()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null) throw new InvalidOperationException("Handler not found");
            return (TResponse)handlerType.GetMethod("Handle").Invoke(handler, new object[] { request }) ?? throw new InvalidOperationException();
        }
    }
};

