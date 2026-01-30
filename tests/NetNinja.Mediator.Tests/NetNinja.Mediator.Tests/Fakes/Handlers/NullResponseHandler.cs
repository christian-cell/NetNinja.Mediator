using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Handlers;

public class NullResponseHandler : IRequestHandler<NullResponseRequest, string>
{
    public Task<string> Handle(NullResponseRequest request, CancellationToken cancellationToken)
        => Task.FromResult<string>(null!);
}