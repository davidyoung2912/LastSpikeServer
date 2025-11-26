using Xunit;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using Moq;

namespace GameplaySessionTracker.Tests
{
    public class SignalRTest
    {
        [Fact]
        public void TestHub()
        {
            var mockHub = new Mock<IHubContext<Hub>>();
            Assert.NotNull(mockHub.Object);
        }
    }
}
