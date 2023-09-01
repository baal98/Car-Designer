using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.DTOs;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class SearchServiceTests
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private ISearchService _service;
        private Guid _projectId;
        private Guid _userId;
        private string _thumbnail;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<ApplicationUser, ApplicationUserDTO>();
            });

            _mapper = config.CreateMapper();
            _service = new SearchService(_context, _mapper);
            _context = new ApplicationDbContext(options);
            _projectId = Guid.NewGuid();
            _userId = Guid.NewGuid();
            _thumbnail = "thumbnail";
        }

        [Test]
        public async Task SearchProjectsAsync_ThrowsArgumentException_WhenSearchTermIsNullOrEmpty()
        {
            // Arrange
            string emptySearchTerm = "";
            string nullSearchTerm = null;

            // Act & Assert
            Func<Task> actEmptySearchTerm = async () => await _service.SearchProjectsAsync(emptySearchTerm);
            Func<Task> actNullSearchTerm = async () => await _service.SearchProjectsAsync(nullSearchTerm);

            await actEmptySearchTerm.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");
            await actNullSearchTerm.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");
        }

        [Test]
        public async Task SearchProjectsAsync_ReturnsListOfProjectSearchViewModel_WhenProjectsExist()
        {
            // Arrange
            string searchTerm = "sample_search_term";
            var sharedProjects = new List<SharedProject>
            {
            };
            _context.SharedProjects.AddRange(sharedProjects);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.SearchProjectsAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ProjectSearchViewModel>>();
        }

        [Test]
        public async Task SearchProjectsAsync_ReturnsEmptyList_WhenNoProjectsFound()
        {
            // Arrange
            string nonExistentSearchTerm = "non_existent_search_term";

            // Act
            var result = await _service.SearchProjectsAsync(nonExistentSearchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task SearchProjectsAsync_ReturnsProjects_WhenProjectsFound()
        {
            // Arrange
            // Add a sample canvas to the database with some objects
            var canvas = new Canvas
            {
                Id = _projectId,
                UserId = _userId,
                Name = "Sample Canvas",
                Thumbnail = _thumbnail,
                Description = "Sample Canvas Description",
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var sharedProject = new SharedProject();
            sharedProject.Canvas = canvas;

            _context.SharedProjects.Add(sharedProject);
            _context.SaveChanges();

            // Act
            var result = await _service.SearchProjectsAsync(canvas.Name);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Test]
        public async Task SearchProjectsAsync_ThrowsArgumentException_WhenSearchTermIsNullOrEmpty1()
        {
            // Arrange
            string emptySearchTerm = "";
            string nullSearchTerm = null;

            // Act & Assert
            Func<Task> emptySearchAct = async () => await _service.SearchProjectsAsync(emptySearchTerm);
            Func<Task> nullSearchAct = async () => await _service.SearchProjectsAsync(nullSearchTerm);

            await emptySearchAct.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");

            await nullSearchAct.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");
        }

        [Test]
        public async Task SearchUsersAsync_ThrowsArgumentException_WhenSearchTermIsNullOrEmpty()
        {
            // Arrange
            string emptySearchTerm = "";
            string nullSearchTerm = null;

            // Act & Assert
            Func<Task> actEmptySearchTerm = async () => await _service.SearchUsersAsync(emptySearchTerm);
            Func<Task> actNullSearchTerm = async () => await _service.SearchUsersAsync(nullSearchTerm);

            await actEmptySearchTerm.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");
            await actNullSearchTerm.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Search term cannot be null or empty");
        }


        [Test]
        public async Task SearchUsersAsync_ReturnsListOfApplicationUsers_WhenUsersExist()
        {
            // Arrange
            var applicationUsers = new List<ApplicationUser>
            {
                new() { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User1" },
                new() { Id = Guid.NewGuid().ToString(), Email = "test9@test.com", UserName = "Test User2" },
                new() { Id = Guid.NewGuid().ToString(), Email = "test10@test.com", UserName = "Test User3" }
            };
            _context.ApplicationUsers.AddRange(applicationUsers);
            await _context.SaveChangesAsync();

            string searchTerm = applicationUsers[0].Email;

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ApplicationUser>>();
            result.Should().HaveCount(1);
        }

        [Test]
        public async Task SearchUsersAsync_ReturnsEmptyList_WhenNoUsersFound()
        {
            // Arrange
            string nonExistentSearchTerm = "non_existent_search_term";

            // Act
            var result = await _service.SearchUsersAsync(nonExistentSearchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
        
    }
}
