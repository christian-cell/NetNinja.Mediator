using System.Threading;
using System.Threading.Tasks;

namespace NetNinja.Mediator.Abstractions
{
    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
};

