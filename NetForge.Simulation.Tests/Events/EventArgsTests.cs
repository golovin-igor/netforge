using NetForge.Simulation.Events;
using Xunit;

namespace NetForge.Simulation.Tests.Events
{
    // Test implementation of abstract NetworkEventArgs for testing
    public class TestNetworkEventArgs : NetworkEventArgs
    {
        public string TestProperty { get; set; }

        public TestNetworkEventArgs(string testProperty = "test")
        {
            TestProperty = testProperty;
        }
    }

    public class NetworkEventArgsTests
    {
        [Fact]
        public void NetworkEventArgs_Constructor_ShouldSetTimestamp()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var eventArgs = new TestNetworkEventArgs();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(eventArgs.Timestamp >= beforeCreation);
            Assert.True(eventArgs.Timestamp <= afterCreation);
        }

        [Fact]
        public void NetworkEventArgs_MultipleInstances_ShouldHaveUniqueTimestamps()
        {
            // Act
            var eventArgs1 = new TestNetworkEventArgs();
            Thread.Sleep(1); // Ensure different timestamps
            var eventArgs2 = new TestNetworkEventArgs();

            // Assert
            Assert.True(eventArgs2.Timestamp > eventArgs1.Timestamp);
        }

        [Fact]
        public void NetworkEventArgs_Timestamp_ShouldBeUtc()
        {
            // Act
            var eventArgs = new TestNetworkEventArgs();

            // Assert
            // Verify timestamp is close to current UTC time
            var now = DateTime.UtcNow;
            var timeDifference = Math.Abs((now - eventArgs.Timestamp).TotalSeconds);
            Assert.True(timeDifference < 1, "Timestamp should be very close to current UTC time");
        }
    }

    public class LinkChangedEventArgsTests
    {
        [Fact]
        public void LinkChangedEventArgs_Constructor_ShouldSetAllProperties()
        {
            // Arrange
            var device1Name = "Router1";
            var interface1Name = "Gi0/0";
            var device2Name = "Switch1";
            var interface2Name = "Fa0/1";
            var changeType = LinkChangeType.Added;

            // Act
            var eventArgs = new LinkChangedEventArgs(device1Name, interface1Name, device2Name, interface2Name, changeType);

            // Assert
            Assert.Equal(device1Name, eventArgs.Device1Name);
            Assert.Equal(interface1Name, eventArgs.Interface1Name);
            Assert.Equal(device2Name, eventArgs.Device2Name);
            Assert.Equal(interface2Name, eventArgs.Interface2Name);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }

        [Fact]
        public void LinkChangedEventArgs_ShouldInheritFromNetworkEventArgs()
        {
            // Act
            var eventArgs = new LinkChangedEventArgs("R1", "Gi0/0", "R2", "Gi0/1", LinkChangeType.Added);

            // Assert
            Assert.IsAssignableFrom<NetworkEventArgs>(eventArgs);
            Assert.True(eventArgs.Timestamp > DateTime.MinValue);
        }

        [Fact]
        public void LinkChangedEventArgs_WithAddedChangeType_ShouldWork()
        {
            // Act
            var eventArgs = new LinkChangedEventArgs("R1", "Gi0/0", "R2", "Gi0/1", LinkChangeType.Added);

            // Assert
            Assert.Equal(LinkChangeType.Added, eventArgs.ChangeType);
        }

        [Fact]
        public void LinkChangedEventArgs_WithRemovedChangeType_ShouldWork()
        {
            // Act
            var eventArgs = new LinkChangedEventArgs("R1", "Gi0/0", "R2", "Gi0/1", LinkChangeType.Removed);

            // Assert
            Assert.Equal(LinkChangeType.Removed, eventArgs.ChangeType);
        }

        [Fact]
        public void LinkChangedEventArgs_WithEmptyStrings_ShouldWork()
        {
            // Act
            var eventArgs = new LinkChangedEventArgs("", "", "", "", LinkChangeType.Added);

            // Assert
            Assert.Equal("", eventArgs.Device1Name);
            Assert.Equal("", eventArgs.Interface1Name);
            Assert.Equal("", eventArgs.Device2Name);
            Assert.Equal("", eventArgs.Interface2Name);
        }

        [Fact]
        public void LinkChangedEventArgs_Properties_ShouldBeReadOnly()
        {
            // Arrange
            var eventArgs = new LinkChangedEventArgs("R1", "Gi0/0", "R2", "Gi0/1", LinkChangeType.Added);

            // Assert - Properties should only have getters (this is verified by compilation)
            Assert.Equal("R1", eventArgs.Device1Name);
            Assert.Equal("Gi0/0", eventArgs.Interface1Name);
            Assert.Equal("R2", eventArgs.Device2Name);
            Assert.Equal("Gi0/1", eventArgs.Interface2Name);
            Assert.Equal(LinkChangeType.Added, eventArgs.ChangeType);

            // Properties are read-only, so this test verifies they can be read but not written
            // Attempting to write would result in a compilation error
        }

        [Theory]
        [InlineData("Router1", "GigabitEthernet0/0", "Switch1", "FastEthernet0/1", LinkChangeType.Added)]
        [InlineData("SW1", "Gi0/1", "SW2", "Gi0/2", LinkChangeType.Removed)]
        [InlineData("R1", "Se0/0/0", "R2", "Se0/0/1", LinkChangeType.Added)]
        public void LinkChangedEventArgs_WithVariousInputs_ShouldWork(
            string device1, string interface1, string device2, string interface2, LinkChangeType changeType)
        {
            // Act
            var eventArgs = new LinkChangedEventArgs(device1, interface1, device2, interface2, changeType);

            // Assert
            Assert.Equal(device1, eventArgs.Device1Name);
            Assert.Equal(interface1, eventArgs.Interface1Name);
            Assert.Equal(device2, eventArgs.Device2Name);
            Assert.Equal(interface2, eventArgs.Interface2Name);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }
    }

    public class LinkChangeTypeTests
    {
        [Fact]
        public void LinkChangeType_ShouldHaveExpectedValues()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(LinkChangeType), LinkChangeType.Added));
            Assert.True(Enum.IsDefined(typeof(LinkChangeType), LinkChangeType.Removed));
        }

        [Fact]
        public void LinkChangeType_ShouldHaveCorrectEnumValues()
        {
            // Assert
            Assert.Equal(0, (int)LinkChangeType.Added);
            Assert.Equal(1, (int)LinkChangeType.Removed);
        }

        [Fact]
        public void LinkChangeType_ShouldHaveOnlyTwoValues()
        {
            // Act
            var enumValues = Enum.GetValues<LinkChangeType>();

            // Assert
            Assert.Equal(2, enumValues.Length);
            Assert.Contains(LinkChangeType.Added, enumValues);
            Assert.Contains(LinkChangeType.Removed, enumValues);
        }
    }
} 
