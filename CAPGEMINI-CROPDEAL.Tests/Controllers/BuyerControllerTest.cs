using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.Interfaces;
using CAPGEMINI_CROPDEAL.DTO;
using CAPGEMINI_CROPDEAL.Models;
using System.Threading.Tasks;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    public class BuyerControllerTests
    {
        private Mock<IUpdateService<Buyer, BuyerDTO>> _mockUpdateService;
        private Mock<ICropSubscriptionService> _mockSubscriptionService;
        private BuyerController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUpdateService = new Mock<IUpdateService<Buyer, BuyerDTO>>();
            _mockSubscriptionService = new Mock<ICropSubscriptionService>();
            _controller = new BuyerController(_mockUpdateService.Object, _mockSubscriptionService.Object);
        }

        [Test]
        public async Task UpdateBuyer_ReturnsOk_WhenBuyerExists()
        {
            // Arrange
            int buyerId = 1;
            var dto = new BuyerDTO
            {
                BuyerName = "John Doe",
                BuyerGmail = "john@example.com",
                PhoneNo = "1234567890"
            };

            // Must return a Buyer, not BuyerDTO
            var buyer = new Buyer
            {
                BuyerId = buyerId,
                BuyerName = dto.BuyerName,
                BuyerGmail = dto.BuyerGmail,
                PhoneNo = dto.PhoneNo
            };

            _mockUpdateService.Setup(s => s.UpdateServiceAsync(buyerId, dto))
                              .ReturnsAsync(buyer);

            // Act
            var result = await _controller.UpdateBuyer(buyerId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(buyer));
        }

        [Test]
        public async Task UpdateBuyer_ReturnsNotFound_WhenBuyerDoesNotExist()
        {
            // Arrange
            int buyerId = 1;
            var dto = new BuyerDTO
            {
                BuyerName = "John Doe",
                BuyerGmail = "john@example.com",
                PhoneNo = "1234567890"
            };

            _mockUpdateService.Setup(s => s.UpdateServiceAsync(buyerId, dto))
                              .ReturnsAsync((Buyer?)null);

            // Act
            var result = await _controller.UpdateBuyer(buyerId, dto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.That(notFoundResult.Value, Is.EqualTo("Buyer not found"));
        }

        [Test]
        public async Task Subscribe_ReturnsOk()
        {
            // Arrange
            int buyerId = 1;
            string cropName = "Wheat";

            _mockSubscriptionService.Setup(s => s.Subscribe(buyerId, cropName))
                                    .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Subscribe(buyerId, cropName);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo("Buyer subscribed to crop"));
        }
    }
}