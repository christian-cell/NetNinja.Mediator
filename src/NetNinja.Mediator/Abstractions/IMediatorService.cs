using System.Threading;
using System.Threading.Tasks;

namespace NetNinja.Mediator.Abstractions
{
    public interface IMediatorService
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
    }
};

