using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Handlers;
public class ExceptionHandler : IRequestHandler<ExceptionRequest, string>
{
    public Task<string> Handle(ExceptionRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(0) 
            .ContinueWith<string>(_ =>
            {
                throw new ApplicationException("Handler error");
            });
    }
}