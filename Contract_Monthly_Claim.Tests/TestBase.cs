// TestBase.cs
using ContractMonthlyClaimSystem;
using ContractMonthlyClaimSystem;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ContractMonthlyClaimSystem
{
    public abstract class TestBase : IDisposable
    {
        protected ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        protected void SeedTestData(ApplicationDbContext context, params object[] entities)
        {
            context.AddRange(entities);
            context.SaveChanges();
        }

        protected Mock<HttpContext> CreateMockHttpContext(string userType = "Lecturer", int userId = 1, string userName = "Test User")
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();

            // Mock session values
            var userIdBytes = BitConverter.GetBytes(userId);
            var userTypeBytes = System.Text.Encoding.UTF8.GetBytes(userType);
            var userNameBytes = System.Text.Encoding.UTF8.GetBytes(userName);

            mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes)).Returns(true);
            mockSession.Setup(s => s.TryGetValue("UserType", out userTypeBytes)).Returns(true);
            mockSession.Setup(s => s.TryGetValue("FullName", out userNameBytes)).Returns(true);

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            return mockHttpContext;
        }

        public void Dispose()
        {
            // Clean up if needed
        }
    }
}