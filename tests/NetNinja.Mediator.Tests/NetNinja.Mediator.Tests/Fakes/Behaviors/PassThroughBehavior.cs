using NetNinja.Mediator.Abstractions;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests.Fakes.Behaviors;

public class PassThroughBehavior : IPipelineBehavior<ExceptionRequest, string>
{
    public Task<string> Handle(ExceptionRequest request, CancellationToken cancellationToken, Func<Task<string>> next)
    {
        return next();
    }
}