namespace NetNinja.Mediator.Abstractions
{
    public interface IMediatorService
    {
        string Greet();
        TResponse Send<TResponse>(IRequest<TResponse> request);
    }
};

