using NetNinja.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Moq;
using NetNinja.Mediator.Enums;
using NetNinja.Mediator.Tests.Fakes.Behaviors;
using NetNinja.Mediator.Tests.Fakes.Handlers;
using NetNinja.Mediator.Tests.Fakes.Requests;

namespace NetNinja.Mediator.Tests
{
    [TestFixture]
    public class MediatorTests
    {
        private IServiceProvider BuildProvider(params Type[] handlerTypes)
        {
            var services = new ServiceCollection();
            var assemblies = handlerTypes.Select(t => t.Assembly).Distinct().ToArray();
            services.AddNetNinjaMediator(true,true,true,true, RegistrationType.None,assemblies);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
            services.AddSingleton(httpContextAccessorMock.Object);

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
            Assert.That(response, Is.EqualTo("A(B(C()))"));
        }

        [Test]
        public void Should_Throw_When_Handler_Not_Registered()
        {
            var services = new ServiceCollection();
            services.AddNetNinjaMediator();

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
            services.AddSingleton(httpContextAccessorMock.Object);

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
        
        [Test]
        public async Task Should_Execute_Pipeline_Behavior()
        {
            PipelineBehavior1.Calls.Clear();

            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();
            services.AddTransient<IRequestHandler<DummyRequest, string>, DummyHandler>();
            services.AddTransient<IPipelineBehavior<DummyRequest, string>, PipelineBehavior1>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var response = await mediator.Send(new DummyRequest());

            Assert.That(response, Is.EqualTo("Hello Mediator + behavior1"));
            Assert.That(PipelineBehavior1.Calls, Is.EqualTo(new[] { "before", "after" }));
        }
        
        [Test]
        public void Should_Throw_When_PipelineBehavior_Returns_Null()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, DummyHandler>();

            services.AddTransient<IPipelineBehavior<DummyRequest, string>, NullBehavior>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator.Send(new DummyRequest()));
        }
        
        [Test]
        public void Should_Unwrap_TargetInvocationException_From_Handler()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<ExceptionRequest, string>, ExceptionHandler>();

            services.AddTransient<IPipelineBehavior<ExceptionRequest, string>, PassThroughBehavior>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var ex = Assert.ThrowsAsync<ApplicationException>(async () =>
                await mediator.Send(new ExceptionRequest()));

            Assert.That(ex!.Message, Is.EqualTo("Handler error"));
        }
        
        [Test]
        public async Task Should_Use_HttpContext_RequestAborted_Token()
        {
            var services = new ServiceCollection();

            var ctsHttp = new CancellationTokenSource();
            var ctsParam = new CancellationTokenSource();

            var context = new DefaultHttpContext();
            context.RequestAborted = ctsHttp.Token;

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            services.AddSingleton(httpContextAccessorMock.Object);
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, CaptureTokenHandler>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            await mediator.Send(new DummyRequest(), ctsParam.Token);

            Assert.That(CaptureTokenHandler.CapturedToken, Is.EqualTo(ctsHttp.Token));
            Assert.That(CaptureTokenHandler.CapturedToken, Is.Not.EqualTo(ctsParam.Token));
        }
        
        [Test]
        public async Task Should_Use_Parameter_CancellationToken_When_HttpContext_Is_Null()
        {
            var services = new ServiceCollection();

            var ctsParam = new CancellationTokenSource();

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);

            services.AddSingleton(httpContextAccessorMock.Object);
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, CaptureTokenHandler2>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            await mediator.Send(new DummyRequest(), ctsParam.Token);

            Assert.That(CaptureTokenHandler2.CapturedToken, Is.EqualTo(ctsParam.Token));
        }
        
        [Test]
        public async Task Should_Execute_Multiple_PipelineBehaviors_In_Registration_Order()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, BaseHandler>();

            services.AddTransient<IPipelineBehavior<DummyRequest, string>, BehaviorA>();
            services.AddTransient<IPipelineBehavior<DummyRequest, string>, BehaviorB>();
            services.AddTransient<IPipelineBehavior<DummyRequest, string>, BehaviorC>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new DummyRequest());

            Assert.That(result, Is.EqualTo("A(B(C(H)))"));
        }
        
        [Test]
        public async Task Should_Execute_Handler_Directly_When_No_PipelineBehaviors()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, DirectHandler>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new DummyRequest());

            Assert.That(result, Is.EqualTo("DIRECT"));
        }
        
        [Test]
        public void Should_Throw_When_Handler_Returns_Null_Two()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            services.AddTransient<IRequestHandler<DummyRequest, string>, NullReturningHandler>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator.Send(new DummyRequest()));

            Assert.That(ex!.Message, Is.EqualTo("Handler returned null"));
        }
        
        [Test]
        public void Should_Throw_When_Handler_Not_Found()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IMediator, Mediator.Services.Mediator>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mediator.Send(new DummyRequest()));

            Assert.That(ex!.Message, Is.EqualTo("Handler not found"));
        }
        
        [Test]
        public void Should_Register_Mediator_And_Accessor_Without_Assemblies()
        {
            var services = new ServiceCollection();

            services.AddNetNinjaMediator(
                autoRegisterValidators: false,
                autoRegisterValidationBehavior: false,
                autoRegisterPipelineBehaviors: false,
                autoRegisterHandlers: false
            );

            var provider = services.BuildServiceProvider();

            Assert.That(provider.GetService<IMediator>(), Is.Not.Null);
            Assert.That(provider.GetService<IHttpContextAccessor>(), Is.Not.Null);

            Assert.That(services.Any(s => s.ServiceType.IsGenericType &&
                                          s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)),
                Is.False);

            Assert.That(services.Any(s => s.ServiceType.IsGenericType &&
                                          s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)),
                Is.False);
        }
    }
}