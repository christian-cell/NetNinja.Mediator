using NetNinja.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Moq;

namespace NetNinja.Mediator.Tests
{
    public class DummyRequest : IRequest<string> { }
    public class DummyHandler : IRequestHandler<DummyRequest, string>
    {
        public Task<string> Handle(DummyRequest request, CancellationToken cancellationToken)
            => Task.FromResult("Hello Mediator");
    }

    public class NullResponseRequest : IRequest<string> { }
    public class NullResponseHandler : IRequestHandler<NullResponseRequest, string>
    {
        public Task<string> Handle(NullResponseRequest request, CancellationToken cancellationToken)
            => Task.FromResult<string>(null!);
    }

    [TestFixture]
    public class MediatorTests
    {
        private IServiceProvider BuildProvider(params Type[] handlerTypes)
        {
            var services = new ServiceCollection();
            var assemblies = handlerTypes.Select(t => t.Assembly).Distinct().ToArray();
            services.AddNetNinjaMediator(assemblies);

            // Mock IHttpContextAccessor para que HttpContext sea null
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);

            return services.BuildServiceProvider();
        }

        [Test]
        public void Should_Resolve_MediatorService()
        {
            var provider = BuildProvider(typeof(DummyHandler));
            var mediator = provider.GetService<IMediator>();
            Assert.IsNotNull(mediator);
        }

        [Test]
        public async Task Should_Invoke_Handler_And_Return_Response()
        {
            var provider = BuildProvider(typeof(DummyHandler));
            var mediator = provider.GetService<IMediator>();
            var response = await mediator!.Send(new DummyRequest(), CancellationToken.None);
            Assert.That(response, Is.EqualTo("Hello Mediator"));
        }

        [Test]
        public void Should_Throw_When_Handler_Not_Registered()
        {
            var services = new ServiceCollection();
            services.AddNetNinjaMediator();

            // Mock IHttpContextAccessor
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediator>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator!.Send(new DummyRequest(), CancellationToken.None));
        }

        [Test]
        public void Should_Throw_When_Handler_Returns_Null()
        {
            var provider = BuildProvider(typeof(NullResponseHandler));
            var mediator = provider.GetService<IMediator>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator!.Send(new NullResponseRequest(), CancellationToken.None));
        }
        
        [Test]
        public void Should_Propagate_Exception_From_Handler()
        {
            var provider = BuildProvider(typeof(ExceptionHandler));
            var mediator = provider.GetService<IMediator>();
            Assert.ThrowsAsync<ApplicationException>(async () =>
                await mediator!.Send(new ExceptionRequest(), CancellationToken.None));
        }
        
        [Test]
        public void Should_Throw_When_Handle_Method_Not_Found()
        {
            var provider = BuildProvider(typeof(NoHandleHandler));
            var mediator = provider.GetService<IMediator>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator!.Send(new NoHandleRequest(), CancellationToken.None));
        }
    }
    
    public class ExceptionRequest : IRequest<string> { }
    public class ExceptionHandler : IRequestHandler<ExceptionRequest, string>
    {
        public Task<string> Handle(ExceptionRequest request, CancellationToken cancellationToken)
            => throw new ApplicationException("Handler error");
    }
    
    public class NoHandleRequest : IRequest<string> { }
    public class NoHandleHandler { }
}