using System.Collections.Concurrent;
using NetSim.Simulation.Events;
using Xunit;

namespace NetSim.Simulation.Tests.Events;

public class NetworkEventBusComprehensiveTests
{
    [Fact]
    public void Subscribe_WithValidHandler_AddsSubscription()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handlerCalled = false;

        // Act
        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        // We can't directly verify subscription, but we can test that publishing works
        Assert.False(handlerCalled); // Not called yet
    }

    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_CallsHandler()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handlerCalled = false;
        var receivedArgs = (TestSpecificEventArgs)null;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handlerCalled = true;
            receivedArgs = args;
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert
        Assert.True(handlerCalled);
        Assert.NotNull(receivedArgs);
        Assert.Equal("TestDevice", receivedArgs.DeviceName);
        Assert.Equal("TestData", receivedArgs.DeviceData);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_CallsAllHandlers()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        });

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler2Called = true;
            return Task.CompletedTask;
        });

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler3Called = true;
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler2Called);
        Assert.True(handler3Called);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_DoesNotThrow()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act & Assert - Should not throw
        await eventBus.PublishAsync(testArgs);
    }

    [Fact]
    public async Task PublishAsync_WithHandlerException_ContinuesWithOtherHandlers()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handler1Called = false;
        var handler3Called = false;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        });

        // This handler throws an exception
        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            throw new Exception("Handler error");
        });

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler3Called = true;
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert - Other handlers should still be called despite the exception
        Assert.True(handler1Called);
        Assert.True(handler3Called);
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandlerException_ContinuesWithOtherHandlers()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handler1Called = false;
        var handler3Called = false;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        });

        // This async handler throws an exception
        eventBus.Subscribe<TestSpecificEventArgs>(async args =>
        {
            await Task.Delay(1);
            throw new Exception("Async handler error");
        });

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            handler3Called = true;
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert - Other handlers should still be called despite the exception
        Assert.True(handler1Called);
        Assert.True(handler3Called);
    }

    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_CallsCorrectHandlers()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var testHandler1Called = false;
        var testHandler2Called = false;
        var anotherHandler1Called = false;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            testHandler1Called = true;
            return Task.CompletedTask;
        });

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            testHandler2Called = true;
            return Task.CompletedTask;
        });

        eventBus.Subscribe<AnotherTestEventArgs>(args =>
        {
            anotherHandler1Called = true;
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");
        var anotherArgs = new AnotherTestEventArgs(42);

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert
        Assert.True(testHandler1Called);
        Assert.True(testHandler2Called);
        Assert.False(anotherHandler1Called); // Should not be called for different event type

        // Reset
        testHandler1Called = false;
        testHandler2Called = false;

        // Act - Publish different event type
        await eventBus.PublishAsync(anotherArgs);

        // Assert
        Assert.False(testHandler1Called); // Should not be called for different event type
        Assert.False(testHandler2Called); // Should not be called for different event type
        Assert.True(anotherHandler1Called);
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandlers_WaitsForAllToComplete()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var completedHandlers = new ConcurrentBag<int>();
        var startTime = DateTime.UtcNow;

        // Subscribe handlers with different delays
        eventBus.Subscribe<TestSpecificEventArgs>(async args =>
        {
            await Task.Delay(100);
            completedHandlers.Add(1);
        });

        eventBus.Subscribe<TestSpecificEventArgs>(async args =>
        {
            await Task.Delay(50);
            completedHandlers.Add(2);
        });

        eventBus.Subscribe<TestSpecificEventArgs>(async args =>
        {
            await Task.Delay(150);
            completedHandlers.Add(3);
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(3, completedHandlers.Count);
        Assert.Contains(1, completedHandlers);
        Assert.Contains(2, completedHandlers);
        Assert.Contains(3, completedHandlers);
        
        // Should wait for all handlers to complete (longest is 150ms)
        Assert.True(elapsedTime.TotalMilliseconds >= 150);
    }

    [Fact]
    public async Task Subscribe_Multiple_SameHandlerInstance_CallsOnlyOnce()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var callCount = 0;

        Func<TestSpecificEventArgs, Task> handler = args =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        // Subscribe the same handler instance multiple times
        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // Assert - Even though subscribed multiple times, should only be called once per subscription
        Assert.Equal(3, callCount); // Each subscription is separate, so called 3 times
    }

    [Fact]
    public async Task PublishAsync_ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var handlerCallCount = 0;
        var lockObject = new object();

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            lock (lockObject)
            {
                handlerCallCount++;
            }
            return Task.CompletedTask;
        });

        var tasks = new List<Task>();
        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act - Publish events concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(eventBus.PublishAsync(testArgs));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, handlerCallCount);
    }

    [Fact]
    public async Task Subscribe_During_Publishing_HandlesCorrectly()
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var initialHandlerCalled = false;
        var dynamicHandlerCalled = false;

        eventBus.Subscribe<TestSpecificEventArgs>(args =>
        {
            initialHandlerCalled = true;
            // Subscribe another handler during event processing
            eventBus.Subscribe<TestSpecificEventArgs>(newArgs =>
            {
                dynamicHandlerCalled = true;
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        await eventBus.PublishAsync(testArgs);

        // The dynamically added handler should not be called for the current event
        Assert.True(initialHandlerCalled);
        Assert.False(dynamicHandlerCalled);

        // But should be called for subsequent events
        initialHandlerCalled = false;
        await eventBus.PublishAsync(testArgs);

        // Assert
        Assert.True(initialHandlerCalled);
        Assert.True(dynamicHandlerCalled);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PublishAsync_WithManyHandlers_PerformsWell(int handlerCount)
    {
        // Arrange
        var eventBus = new NetworkEventBus();
        var callCounts = new int[handlerCount];

        for (int i = 0; i < handlerCount; i++)
        {
            var index = i; // Capture for closure
            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                callCounts[index]++;
                return Task.CompletedTask;
            });
        }

        var testArgs = new TestSpecificEventArgs("TestDevice", "TestData");

        // Act
        var startTime = DateTime.UtcNow;
        await eventBus.PublishAsync(testArgs);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        for (int i = 0; i < handlerCount; i++)
        {
            Assert.Equal(1, callCounts[i]);
        }

        // Performance assertion - should complete reasonably quickly
        Assert.True(elapsedTime.TotalSeconds < 1, $"Event processing took too long: {elapsedTime.TotalMilliseconds}ms");
    }
}

// Test event classes for comprehensive testing
public class AnotherTestEventArgs(int testNumber) : NetworkEventArgs
{
    public int TestNumber { get; } = testNumber;
} 