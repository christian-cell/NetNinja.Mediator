using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Handlers;

public class DummyHandler : IRequestHandler<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken)
        => Task.FromResult("Hello Mediator");
}

public class DummyBeautifulHandler : IRequestHandler<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken)
        => Task.FromResult("OK");
}
