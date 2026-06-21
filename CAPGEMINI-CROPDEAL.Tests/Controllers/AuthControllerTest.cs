using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.Interfaces;
using CAPGEMINI_CROPDEAL.DTO;
using System.Threading.Tasks;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    public class AuthControllerTests
    {
        private Mock<IAuthService> _mockService;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IAuthService>();
            _controller = new AuthController(_mockService.Object);
        }

        [Test]
        public async Task Register_ReturnsOkWithResult()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "password123",
                PhoneNo = "1234567890",
                Location = "City",
                Role = "User"
            };

            var expectedResult = "User registered successfully";

            _mockService.Setup(s => s.Register(registerDto))
                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task Login_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "john@example.com",
                Password = "password123"
            };

            var expectedToken = "mocked-jwt-token";

            _mockService.Setup(s => s.Login(loginDto))
                        .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedToken));
        }
    }
}