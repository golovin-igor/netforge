using NetSim.Simulation.Events;
using Xunit;
// From the main project

// Added for xUnit

namespace NetSim.Simulation.Tests.Events // Adjusted namespace for the test project
{
    // A simple custom event args class for testing the bus directly
    public class TestSpecificEventArgs(string deviceName, string deviceData) : NetworkEventArgs
    {
        public string DeviceName { get; set; } = deviceName;

        public string DeviceData { get; set; } = deviceData;
    }

    public class NetworkEventBusTests
    {
        [Fact]
        public async Task Publish_SubscribedHandlerReceivesEvent_ShouldPass()
        {
            // Arrange
            var eventBus = new NetworkEventBus();
            bool eventReceived = false;
            string receivedMessage = null;

            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                eventReceived = true;
                receivedMessage = args.DeviceData;
                return Task.CompletedTask;
            });

            // Act
            await eventBus.PublishAsync(new TestSpecificEventArgs("test", "Hello EventBus!"));

            // Assert
            Assert.True(eventReceived, "Event was not received.");
            Assert.Equal("Hello EventBus!", receivedMessage);
        }

        [Fact]
        public async Task Publish_MultipleSubscribersReceiveEvent_ShouldPass()
        {
            // Arrange
            var eventBus = new NetworkEventBus();
            int handler1ReceivedCount = 0;
            int handler2ReceivedCount = 0;

            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                handler1ReceivedCount++;
                return Task.CompletedTask;
            });
            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                handler2ReceivedCount++;
                return Task.CompletedTask;
            });

            // Act
            await eventBus.PublishAsync(new TestSpecificEventArgs("test", "Multi-subscriber test"));

            // Assert
            Assert.Equal(1, handler1ReceivedCount);
            Assert.Equal(1, handler2ReceivedCount);
        }

        [Fact]
        public async Task Publish_NoSubscribers_ShouldNotThrow()
        {
            // Arrange
            var eventBus = new NetworkEventBus();

            // Act & Assert
            // Record.ExceptionAsync can be used if we expect PublishAsync to potentially throw, 
            // but here we expect it to complete without throwing.
            await eventBus.PublishAsync(new TestSpecificEventArgs("test", "No subscribers"));
            // If it completes without an exception, the test passes implicitly for this condition.
            Assert.True(true); // Explicit assertion for clarity
        }

        [Fact]
        public async Task Publish_HandlerThrowsException_ShouldNotStopOtherHandlersOrBus()
        {
            // Arrange
            var eventBus = new NetworkEventBus();
            bool handler1Called = false;
            bool handler2Attempted = false; // To check if the handler that throws was entered
            bool handler3CalledAfterException = false;

            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                handler1Called = true;
                return Task.CompletedTask;
            });

            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                handler2Attempted = true;
                throw new InvalidOperationException("Simulated handler exception");
            });

            eventBus.Subscribe<TestSpecificEventArgs>(args =>
            {
                handler3CalledAfterException = true;
                return Task.CompletedTask;
            });

            // Act
            await eventBus.PublishAsync(new TestSpecificEventArgs("test", "Exception test"));

            // Assert
            Assert.True(handler1Called, "Handler 1 should have been called.");
            Assert.True(handler2Attempted, "Handler 2 (that throws) should have been attempted.");
            Assert.True(handler3CalledAfterException, "Handler 3 (after exception) should have been called.");
        }
    }
}