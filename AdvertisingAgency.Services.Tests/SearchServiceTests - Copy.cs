//using NUnit.Framework;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using AutoMapper;
//using AdvertisingAgency.Data.Data;
//using AdvertisingAgency.Data.Data.Models;
//using AdvertisingAgency.Services.Interfaces;
//using AdvertisingAgency.Web.ViewModels.DTOs;
//using AdvertisingAgency.Web.ViewModels.ViewModels;

//namespace AdvertisingAgency.Services.Tests
//{
//    [TestFixture]
//    public class SearchServiceTests
//    {
//        // Test data
//        private List<SharedProject> sharedProjects;
//        private List<ApplicationUser> users;
//        private List<Canvas> canvases;

//        [SetUp]
//        public void SetUp()
//        {
//            // Initialize test data
//            sharedProjects = new List<SharedProject>
//            {
//                new SharedProject { CanvasId = Guid.Parse("6492739B-168A-4E58-A875-03480E716EA1"), SharingUserId = Guid.Parse("1D9BA624-CF9D-4773-A656-4F39F67AAC3A") },
//                new SharedProject { CanvasId = Guid.Parse("D683BE5E-09D7-4DA1-B948-03891760F100"), SharingUserId = Guid.Parse("381C78FE-4A61-43A7-BF05-830C76C43DF8") },
//            };

//            users = new List<ApplicationUser>
//            {
//                new ApplicationUser { Id = "DACA8C46-47E5-40AF-B453-422032A97923", Email = "user1@example.com", PhoneNumber = "+123456789" },
//                new ApplicationUser { Id = "DACA8C46-47E5-40AF-B453-422032A97923", Email = "user2@example.com", PhoneNumber = "+987654321" },
//            };

//            canvases = new List<Canvas>
//            {
//                new Canvas { Id = Guid.Parse("6492739B-168A-4E58-A875-03480E716EA1"), Name = "Project 1", Description = "Test project 1", Thumbnail = "thumbnail1.jpg" },
//                new Canvas { Id = Guid.Parse("D683BE5E-09D7-4DA1-B948-03891760F100"), Name = "Project 2", Description = "Test project 2", Thumbnail = "thumbnail2.jpg" },
//            };
//        }

//        [Test]
//        public async Task SearchProjectsAsync_ValidSearchTerm_ReturnsMatchingProjects()
//        {
//            // Arrange
//            string searchTerm = "Test";
//            var dbContextMock = new Mock<ApplicationDbContext>();
//            dbContextMock.Setup(m => m.SharedProjects).Returns(sharedProjects.AsQueryable());
//            dbContextMock.Setup(m => m.Canvases).Returns(canvases.AsQueryable());
//            dbContextMock.Setup(m => m.ApplicationUsers).Returns(users.AsQueryable());

//            var searchService = new SearchService(dbContextMock.Object, GetMapper());

//            // Act
//            var result = await searchService.SearchProjectsAsync(searchTerm);

//            // Assert
//            Assert.AreEqual(2, result.Count);
//            CollectionAssert.AreEquivalent(new[] { 1, 2 }, result.Select(p => p.Id));
//        }

//        [Test]
//        public void SearchProjectsAsync_EmptySearchTerm_ThrowsArgumentException()
//        {
//            // Arrange
//            string searchTerm = string.Empty;
//            var dbContextMock = new Mock<ApplicationDbContext>();

//            var searchService = new SearchService(dbContextMock.Object, GetMapper());

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentException>(async () => await searchService.SearchProjectsAsync(searchTerm));
//        }

//        [Test]
//        public void SearchProjectsAsync_NullSearchTerm_ThrowsArgumentException()
//        {
//            // Arrange
//            string searchTerm = null;
//            var dbContextMock = new Mock<ApplicationDbContext>();

//            var searchService = new SearchService(dbContextMock.Object, GetMapper());

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentException>(async () => await searchService.SearchProjectsAsync(searchTerm));
//        }

//        // Add more test methods for other scenarios, like no projects found, invalid search term, etc.

//        private IMapper GetMapper()
//        {
//            var mapperConfig = new MapperConfiguration(config =>
//            {
//                config.CreateMap<ApplicationUser, ApplicationUserDTO>();
//            });

//            return mapperConfig.CreateMapper();
//        }
//    }
//}
