using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.Data;
using CAPGEMINI_CROPDEAL.Models;
using CAPGEMINI_CROPDEAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using System.Linq;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    [TestFixture]
    public class PaymentControllerTest
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private CropDealDbContext _context;
        private PaymentController _controller;

        [SetUp]
        public void Setup()
        {
            _paymentServiceMock = new Mock<IPaymentService>();

            var options = new DbContextOptionsBuilder<CropDealDbContext>()
                .UseInMemoryDatabase(databaseName: "PaymentTestDb")
                .Options;
            _context = new CropDealDbContext(options);

            // Seed some test data
            var farmer = new Farmer { FarmerId = 1, UserId = "farmer-1", FarmerName = "Test Farmer" };
            _context.Farmers.Add(farmer);

            var buyer = new Buyer { BuyerId = 1, UserId = "buyer-1", BuyerName = "Test Buyer" };
            _context.Buyers.Add(buyer);

            _context.Invoices.Add(new Invoice
            {
                Id = 1,
                OrderId = 1,
                Order = new Order { Id = 1, FarmerId = 1 },
                BuyerName = "Test Buyer",
                CropName = "Wheat",
                Quantity = 10,
                PricePerUnit = 50,
                TotalAmount = 500,
                GeneratedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            _controller = new PaymentController(_paymentServiceMock.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task Pay_ReturnsOk_WithPaymentResult()
        {
            // Arrange
            int orderId = 1;
            _paymentServiceMock.Setup(s => s.PayForOrder(orderId))
                .ReturnsAsync("Payment Successful");

            // Act
            var result = await _controller.Pay(orderId) as Microsoft.AspNetCore.Mvc.OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Payment Successful", result.Value);
        }

        [Test]
        public async Task GetFarmerReceipts_ReturnsOk_WithReceipts()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "farmer-1"),
                new Claim(ClaimTypes.Role, "Farmer")
            }, "mock"));
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetFarmerReceipts() as Microsoft.AspNetCore.Mvc.OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var receipts = result.Value as IEnumerable<object>;
            Assert.IsNotNull(receipts);
            Assert.AreEqual(1, receipts.Count());
        }

        [Test]
        public async Task GetFarmerReceipts_ReturnsNotFound_WhenFarmerDoesNotExist()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "nonexistent-farmer"),
                new Claim(ClaimTypes.Role, "Farmer")
            }, "mock"));
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetFarmerReceipts() as Microsoft.AspNetCore.Mvc.NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Farmer not found", result.Value);
        }
    }
}