using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    public class AdminControllerTests
    {
        private Mock<IAdminService> _mockService; 
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            //_mockService does not implement the IAdminServiec
            _mockService = new Mock<IAdminService>(); // _mockService is reponsible for creating fake senarios for testing the controller. 
            _controller = new AdminController(_mockService.Object);//this is the object that implements the IAdminService that the controller can interact with.
        }

        [Test]
        public async Task ActivateFarmer_ReturnsOk()
        {
            // Arrange
            int farmerId = 1;
            _mockService.Setup(s => s.ActivateFarmer(farmerId))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ActivateFarmer(farmerId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo("Farmer Activated"));
        }

        [Test]
        public async Task DeactivateFarmer_ReturnsOk()
        {
            // Arrange
            int farmerId = 1;
            _mockService.Setup(s => s.DeactivateFarmer(farmerId))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeactivateFarmer(farmerId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo("Farmer Deactivated"));
        }

        [Test]
        public async Task ExportFarmerPerformance_ReturnsFile()
        {
            // Arrange
            var mockData = new List<FarmerPerformanceDto>
            {
                new FarmerPerformanceDto
                {
                    FarmerName = "Farmer1",
                    TotalCrops = 5,
                    TotalOrders = 10,
                    AverageRating = 4.5
                }
            };

            var mockExcel = new byte[] { 0x0, 0x1, 0x2 };

            _mockService.Setup(s => s.GetFarmerPerformance())
                        .ReturnsAsync(mockData);

            _mockService.Setup(s => s.GenerateExcel(mockData))
                        .Returns(mockExcel);

            // Act
            var result = await _controller.ExportFarmerPerformance();

            // Assert
            var fileResult = result as FileContentResult;
            Assert.IsNotNull(fileResult);
            Assert.That(fileResult.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("FarmerPerformance.xlsx"));
            Assert.That(fileResult.FileContents, Is.EqualTo(mockExcel));
        }
    }
}