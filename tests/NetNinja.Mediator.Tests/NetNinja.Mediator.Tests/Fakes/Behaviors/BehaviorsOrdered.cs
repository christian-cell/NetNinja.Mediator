using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Behaviors;

public class BehaviorsOrdered
{
    
}

public class BehaviorA : IPipelineBehavior<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
        => Task.FromResult("A(" + next().Result + ")");
}

public class BehaviorB : IPipelineBehavior<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
        => Task.FromResult("B(" + next().Result + ")");
}

public class BehaviorC : IPipelineBehavior<DummyRequest, string>
{
    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
        => Task.FromResult("C(" + next().Result + ")");
}