using NetNinja.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace NetNinja.Mediator.Tests
{
    public class DummyRequest : IRequest<string> { }
    public class DummyHandler : IRequestHandler<DummyRequest, string>
    {
        public string Handle(DummyRequest request) => "Hello Mediator";
    }

    public class NullResponseRequest : IRequest<string> { }
    public class NullResponseHandler : IRequestHandler<NullResponseRequest, string>
    {
        public string Handle(NullResponseRequest request) => null;
    }

    [TestFixture]
    public class MediatorServiceTests
    {
        private IServiceProvider BuildProvider(params Type[] handlerTypes)
        {
            var services = new ServiceCollection();
            var assemblies = handlerTypes.Select(t => t.Assembly).Distinct().ToArray();
            services.AddNetNinjaMediator(assemblies);
            return services.BuildServiceProvider();
        }

        [Test]
        public void Should_Resolve_MediatorService()
        {
            var provider = BuildProvider(typeof(DummyHandler));
            var mediator = provider.GetService<IMediatorService>();
            Assert.IsNotNull(mediator);
        }

        [Test]
        public void Should_Invoke_Handler_And_Return_Response()
        {
            var provider = BuildProvider(typeof(DummyHandler));
            var mediator = provider.GetService<IMediatorService>();
            var response = mediator.Send(new DummyRequest());
            Assert.AreEqual("Hello Mediator", response);
        }

        [Test]
        public void Should_Throw_When_Handler_Not_Registered()
        {
            var services = new ServiceCollection();
            services.AddNetNinjaMediator();
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediatorService>();
            Assert.Throws<InvalidOperationException>(() => mediator.Send(new DummyRequest()));
        }

        [Test]
        public void Should_Throw_When_Handler_Returns_Null()
        {
            var provider = BuildProvider(typeof(NullResponseHandler));
            var mediator = provider.GetService<IMediatorService>();
            Assert.Throws<InvalidOperationException>(() => mediator.Send(new NullResponseRequest()));
        }

        [Test]
        public void Greet_Should_Throw_NotImplementedException()
        {
            var provider = BuildProvider(typeof(DummyHandler));
            var mediator = provider.GetService<IMediatorService>();
            Assert.Throws<NotImplementedException>(() => mediator.Greet());
        }
    }
}