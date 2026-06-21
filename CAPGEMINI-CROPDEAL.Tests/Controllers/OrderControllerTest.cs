using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.DTO;
using CAPGEMINI_CROPDEAL.Interfaces;
using CAPGEMINI_CROPDEAL.Data;
using CAPGEMINI_CROPDEAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    public class OrderControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private CropDealDbContext _context;
        private OrderController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();

            // InMemory DB setup
            var options = new DbContextOptionsBuilder<CropDealDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new CropDealDbContext(options);

            // Seed a buyer
            _context.Buyers.Add(new Buyer
            {
                BuyerId = 1,
                BuyerName = "Test Buyer",
                UserId = "user-123",
                BuyerGmail = "test@buyer.com",
                PhoneNo = "1234567890"
            });
            _context.SaveChanges();

            _controller = new OrderController(_mockOrderService.Object, _context);

            // Mock authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Role, "Buyer")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateOrder_ReturnsOk_WithOrderResponse()
        {
            // Arrange
            var createDto = new CreateOrderDto { CropId = 10, Quantity = 2 };
            var order = new Order
            {
                Id = 100,
                CropId = 10,
                Quantity = 2,
                TotalPrice = 500,
                FarmerId = 5
            };

            _mockOrderService
                .Setup(s => s.CreateOrder(1, createDto))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.CreateOrder(createDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as OrderResponseDto;
            Assert.NotNull(response);
            Assert.AreEqual(order.Id, response.OrderId);
            Assert.AreEqual(order.CropId, response.CropId);
            Assert.AreEqual(order.Quantity, response.Quantity);
            Assert.AreEqual(order.TotalPrice, response.TotalPrice);
            Assert.AreEqual(order.FarmerId, response.FarmerId);
        }

        [Test]
        public async Task CreateOrder_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // empty user
            var createDto = new CreateOrderDto { CropId = 1, Quantity = 1 };

            // Act
            var result = await _controller.CreateOrder(createDto);

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

        [Test]
        public async Task CreateOrder_ReturnsBadRequest_WhenBuyerNotFound()
        {
            // Arrange
            var createDto = new CreateOrderDto { CropId = 1, Quantity = 1 };
            // Change user ID to one that does not exist in DB
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "non-existent-user")
            }));

            // Act
            var result = await _controller.CreateOrder(createDto);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.NotNull(badRequest);
            Assert.AreEqual("Buyer not found", badRequest.Value);
        }
    }
}