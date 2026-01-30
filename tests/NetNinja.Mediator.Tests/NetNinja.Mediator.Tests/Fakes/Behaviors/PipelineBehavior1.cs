using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Behaviors;

public class PipelineBehavior1 : IPipelineBehavior<DummyRequest, string>
{
    public static List<string> Calls = new();

    public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
    {
        Calls.Add("before");
        var result = next().Result;
        Calls.Add("after");
        return Task.FromResult(result + " + behavior1");
    }
}