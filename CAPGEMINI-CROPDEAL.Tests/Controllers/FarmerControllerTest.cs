using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CAPGEMINI_CROPDEAL.Controllers;
using CAPGEMINI_CROPDEAL.Interfaces;
using CAPGEMINI_CROPDEAL.DTO;
using CAPGEMINI_CROPDEAL.Models;
using CAPGEMINI_CROPDEAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CAPGEMINI_CROPDEAL.Tests.Controllers
{
    public class FarmerControllerTests
    {
        private Mock<IUpdateService<Farmer, FarmerDTO>> _mockUpdateService;
        private Mock<IAddCropService> _mockAddCropService;
        private Mock<IUpdateCropService> _mockUpdateCropService;
        private Mock<ICropDeleteService> _mockDeleteService;
        private CropDealDbContext _context;
        private FarmerController _controller;
        private Farmer _farmer;

        [SetUp]
        public void Setup()
        {
            _mockUpdateService = new Mock<IUpdateService<Farmer, FarmerDTO>>();
            _mockAddCropService = new Mock<IAddCropService>();
            _mockUpdateCropService = new Mock<IUpdateCropService>();
            _mockDeleteService = new Mock<ICropDeleteService>();

            // In-memory DbContext
            var options = new DbContextOptionsBuilder<CropDealDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new CropDealDbContext(options);

            // Seed a farmer
            _farmer = new Farmer
            {
                FarmerId = 1,
                FarmerName = "John",
                FarmerGmail = "john@example.com",
                UserId = "user123",
                IsActive = true
            };
            _context.Farmers.Add(_farmer);
            _context.SaveChanges();

            _controller = new FarmerController(
                _mockUpdateService.Object,
                _mockAddCropService.Object,
                _mockUpdateCropService.Object,
                _mockDeleteService.Object,
                _context
            );

            // Mock User claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
            };
        }

        [TearDown]//cleans the in-MemeoryDB after each test
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();  
        }

        [Test]
        public async Task UpdateFarmer_ReturnsOk_WhenFarmerExists()
        {
            var dto = new FarmerDTO
            {
                FarmerName = "Updated John",
                FarmerGmail = "updated@example.com",
                PhoneNo = "1234567890",
                Location = "Farmville"
            };

            var updatedFarmer = new Farmer
            {
                FarmerId = _farmer.FarmerId,
                FarmerName = dto.FarmerName,
                FarmerGmail = dto.FarmerGmail,
                PhoneNo = dto.PhoneNo,
                Location = dto.Location,
                UserId = _farmer.UserId
            };

            _mockUpdateService.Setup(s => s.UpdateServiceAsync(_farmer.FarmerId, dto))
                              .ReturnsAsync(updatedFarmer);

            var result = await _controller.UpdateFarmer(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(updatedFarmer, okResult.Value);
        }

        [Test]
        public async Task UpdateFarmer_ReturnsNotFound_WhenFarmerDoesNotExist()
        {
            // Remove farmer from Db
            _context.Farmers.Remove(_farmer);
            _context.SaveChanges();

            var dto = new FarmerDTO();

            var result = await _controller.UpdateFarmer(dto);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Farmer not found", notFoundResult.Value);
        }

        [Test]
        public async Task AddCrop_ReturnsOk_WhenFarmerIsActive()
        {
            var dto = new CropDTO
            {
                CropName = "Wheat",
                CropType = "Grain",
                CropPrice = 10,
                Quantity = 100
            };

            _mockAddCropService.Setup(s => s.Add(_farmer.FarmerId, dto))
                               .Returns(Task.CompletedTask);

            var result = await _controller.AddCrop(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("The crop was successfully added.", okResult.Value);
        }

        [Test]
        public void AddCrop_ThrowsException_WhenFarmerInactive()
        {
            _farmer.IsActive = false;
            _context.SaveChanges();

            var dto = new CropDTO();

            Assert.ThrowsAsync<Exception>(async () => await _controller.AddCrop(dto), "Farmer is InActive");
        }

        [Test]
        public async Task UpdateCrop_ReturnsOk_WhenCropExists()
        {
            var crop = new Crop
            {
                CropId = 1,
                CropName = "Wheat",
                CropType = "Grain",
                CropPrice = 10,
                Quantity = 100,
                FarmerId = _farmer.FarmerId
            };
            _context.Crops.Add(crop);
            _context.SaveChanges();

            var dto = new CropDTO { CropName = "Updated Wheat", CropType = "Grain", CropPrice = 12, Quantity = 80 };
            var message = "Crop Updated Successfully.";
            _mockUpdateCropService.Setup(s => s.Update(_farmer.FarmerId, crop.CropId, dto))
                                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateCrop(crop.CropId, dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(message,Is.EqualTo( okResult.Value ));
        }

        [Test]
        public async Task DeleteCrop_ReturnsOk_WhenCropExists()
        {
            var crop = new Crop { CropId = 1, FarmerId = _farmer.FarmerId };
            _context.Crops.Add(crop);
            _context.SaveChanges();

            _mockDeleteService.Setup(s => s.Delete(crop.CropId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteCrop(crop.CropId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Crop deleted successfully", okResult.Value);
        }

        [Test]
        public async Task DeleteCrop_ReturnsNotFound_WhenCropDoesNotExist()
        {
            var result = await _controller.DeleteCrop(999); 

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Crop not found for this farmer", notFoundResult.Value);
        }
    }
}