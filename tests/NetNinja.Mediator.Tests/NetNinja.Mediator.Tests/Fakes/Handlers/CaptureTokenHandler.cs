using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Handlers;

public class CaptureTokenHandler : IRequestHandler<DummyRequest, string>
{
    public static CancellationToken CapturedToken;

    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken)
    {
        CapturedToken = cancellationToken;
        return Task.FromResult("OK");
    }
}