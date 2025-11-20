// Controllers/AccountControllerTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem;
using ContractMonthlyClaimSystem.Models;
using Xunit;

namespace ContractMonthlyClaimSystem
{
    public class AccountControllerTests : TestBase
    {
        private readonly AccountController _controller;
        private readonly ApplicationDbContext _context;

        public AccountControllerTests()
        {
            _context = GetInMemoryDbContext();
            _controller = new AccountController(_context);
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<LoginViewModel>();
        }

        [Fact]
        public async Task Login_Post_WithValidCredentials_RedirectsToDashboard()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                Password = "password123",
                UserType = "Lecturer",
                FullName = "Test User",
                Email = "test@email.com"
            };
            SeedTestData(_context, user);

            var model = new LoginViewModel
            {
                Username = "testuser",
                Password = "password123",
                UserType = "Lecturer"
            };

            // Mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            var result = await _controller.Login(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("index");
            redirectResult.ControllerName.Should().Be("Dashboard");
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = "invaliduser",
                Password = "wrongpassword",
                UserType = "Lecturer"
            };

            // Act
            var result = await _controller.Login(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.IsValid.Should().BeFalse();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<RegisterViewModel>();
        }

        [Fact]
        public async Task Register_Post_WithValidData_CreatesUserAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                ConfirmPassword = "password123",
                Email = "newuser@email.com",
                UserType = "Lecturer",
                FullName = "New User"
            };

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Login");

            // Verify user was created
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            user.Should().NotBeNull();
            user.FullName.Should().Be("New User");
            user.UserType.Should().Be("Lecturer");
        }

        [Fact]
        public async Task Register_Post_WithExistingUsername_ReturnsViewWithError()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "existinguser",
                Password = "password123",
                UserType = "Lecturer",
                FullName = "Existing User",
                Email = "existing@email.com"
            };
            SeedTestData(_context, existingUser);

            var model = new RegisterViewModel
            {
                Username = "existinguser", // Same username
                Password = "password123",
                ConfirmPassword = "password123",
                Email = "new@email.com",
                UserType = "Lecturer",
                FullName = "New User"
            };

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.IsValid.Should().BeFalse();
            _controller.ModelState["Username"].Errors.Should().Contain(e => e.ErrorMessage == "Username already exists. Please choose a different one.");
        }

        [Fact]
        public void Logout_ClearsSessionAndRedirects()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            var result = _controller.Logout();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Login");

            mockSession.Verify(s => s.Clear(), Times.Once);
        }

        public new void Dispose()
        {
            _context?.Dispose();
            base.Dispose();
        }
    }
}