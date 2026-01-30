using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Behaviors;

public class NullBehavior : IPipelineBehavior<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
    {
        return Task.FromResult<string>(null!);
    }
}