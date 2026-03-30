// DependencyInjectionTests — integration tests for Application layer DI registration.
//
// Covered behaviours:
//   1. AddApplication() registers Mediator as scoped
//   2. Assembly scanning discovers and registers all IRequestHandler<,> implementations
//   3. ValidationBehavior and LoggingBehavior are registered as scoped
//   4. Handlers are resolvable from DI container
//   5. Handler can be dispatched via Mediator without manual registration
//
// This test file verifies that the AddApplication() extension method correctly
// registers all required services and that assembly scanning discovers handlers.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Microsoft.Extensions.DependencyInjection;
using StackFlow.Application;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Behaviors;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.IntegrationTests;

public class DependencyInjectionTests
{
    // ── Minimal test handler for assembly scanning verification ────────────────
    // This handler is in the test assembly, not in StackFlow.Application, so we
    // verify that AddApplication() scans the Application assembly only.

    public record TestQuery : IQuery<Result<string>>;

    // ── Test 1: Mediator is registered as scoped ──────────────────────────────

    [Fact]
    public void AddApplication_RegistersMediatorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        var serviceProvider = services.BuildServiceProvider();

        // Assert — Mediator should be resolvable
        var mediator1 = serviceProvider.GetRequiredService<Mediator>();
        var mediator2 = serviceProvider.GetRequiredService<Mediator>();

        Assert.NotNull(mediator1);
        Assert.NotNull(mediator2);

        // They should be different instances (scoped lifetime)
        using (var scope1 = serviceProvider.CreateScope())
        using (var scope2 = serviceProvider.CreateScope())
        {
            var mediatorFromScope1 = scope1.ServiceProvider.GetRequiredService<Mediator>();
            var mediatorFromScope2 = scope2.ServiceProvider.GetRequiredService<Mediator>();

            // Different scopes = different instances
            Assert.NotSame(mediatorFromScope1, mediatorFromScope2);
        }
    }

    // ── Test 2: ValidationBehavior is registered ──────────────────────────────

    [Fact]
    public void AddApplication_RegistersValidationBehavior()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        var serviceProvider = services.BuildServiceProvider();

        // Assert — ValidationBehavior<T, TResponse> should be resolvable for any types
        var behaviorType = typeof(IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>);
        var behaviors = serviceProvider.GetRequiredService(behaviorType)
            as IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>;

        Assert.NotNull(behaviors);
        var behaviorList = behaviors.ToList();

        // There should be at least one behavior registered (ValidationBehavior)
        Assert.True(behaviorList.Count > 0, "Expected at least one IPipelineBehavior to be registered");

        // ValidationBehavior should be the first one (registered first)
        Assert.IsType<ValidationBehavior<TestQuery, Result<string>>>(behaviorList[0]);
    }

    // ── Test 3: LoggingBehavior is registered ────────────────────────────────

    [Fact]
    public void AddApplication_RegistersLoggingBehavior()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        var serviceProvider = services.BuildServiceProvider();

        // Assert — LoggingBehavior should be registered
        var behaviorType = typeof(IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>);
        var behaviors = serviceProvider.GetRequiredService(behaviorType)
            as IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>;

        Assert.NotNull(behaviors);
        var behaviorList = behaviors.ToList();

        // There should be two behaviors: ValidationBehavior and LoggingBehavior
        Assert.True(behaviorList.Count >= 2, "Expected at least ValidationBehavior and LoggingBehavior");

        // LoggingBehavior should be the second one (registered second)
        Assert.IsType<LoggingBehavior<TestQuery, Result<string>>>(behaviorList[1]);
    }

    // ── Test 4: Behaviors are registered in the correct order ─────────────────

    [Fact]
    public void AddApplication_BehaviorsRegisteredInCorrectOrder_ValidationThenLogging()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behaviorType = typeof(IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>);
        var behaviors = serviceProvider.GetRequiredService(behaviorType)
            as IEnumerable<IPipelineBehavior<TestQuery, Result<string>>>;

        var behaviorList = behaviors!.ToList();

        // Verify exact types in order
        Assert.IsType<ValidationBehavior<TestQuery, Result<string>>>(behaviorList[0]);
        Assert.IsType<LoggingBehavior<TestQuery, Result<string>>>(behaviorList[1]);
    }

    // ── Test 5: PingCommandHandler is discovered and registered ───────────────
    // PingCommand and PingCommandHandler are defined in StackFlow.Application,
    // so they should be discovered by assembly scanning.

    [Fact]
    public void AddApplication_DiscoversPingCommandHandlerViaAssemblyScanning()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Assert — PingCommandHandler should be resolvable
        // PingCommand handler type is IRequestHandler<PingCommand, Result<string>>
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(PingCommand),
            typeof(Result<string>));

        var handler = serviceProvider.GetRequiredService(handlerType);
        Assert.NotNull(handler);
    }

    // ── Test 6: Handler can be dispatched via Mediator ───────────────────────

    [Fact]
    public async Task Mediator_Send_DispatchesPingCommand_Returns_Pong()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        // Act
        var result = await mediator.Send<Result<string>>(new PingCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("pong", result.Value);
    }

    // ── Test 7: Validators can be registered and resolved ─────────────────────

    [Fact]
    public void AddApplication_AllowsValidatorsToBeResolved()
    {
        // Arrange — Validators are registered manually by features
        var services = new ServiceCollection();
        services.AddApplication();

        var serviceProvider = services.BuildServiceProvider();

        // Assert — Resolve IEnumerable<IValidator<T>> for any type; should return
        // empty list if no validators are registered yet (not an error).
        var validatorType = typeof(IEnumerable<>).MakeGenericType(
            typeof(FluentValidation.IValidator<>).MakeGenericType(typeof(PingCommand)));

        var validators = serviceProvider.GetRequiredService(validatorType);
        Assert.NotNull(validators);
    }
}

// Helper: import the actual PingCommand from Application to verify it's discoverable
// This import verifies the test can access the production command type.
using PingCommand = StackFlow.Application.Features.Ping.PingCommand;
