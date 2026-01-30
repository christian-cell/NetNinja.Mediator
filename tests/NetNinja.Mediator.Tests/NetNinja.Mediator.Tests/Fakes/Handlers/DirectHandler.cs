using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Handlers;

public class DirectHandler : IRequestHandler<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken)
        => Task.FromResult("DIRECT");
}