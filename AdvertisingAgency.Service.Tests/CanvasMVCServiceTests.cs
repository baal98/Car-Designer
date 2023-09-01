using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Data.Models.Canvas;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class CanvasMVCServiceTests
    {
        private ApplicationDbContext _context;
        private ICanvasMVCService _service;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _service = new CanvasMVCService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        // Test for method GetCanvasProjects
        [Test]
        public async Task GetCanvasProjects_ShouldReturnCorrectProjects_WhenUserHasCanvases()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;

            _context.ApplicationUsers.Add(user);
            _context.SaveChanges();

            var canvasList = new List<Canvas>
            {
                new Canvas {
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = Guid.Parse(userId),
                    CreatedOn = DateTime.UtcNow,
                    Name = user.FriendlyName,
                    Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }},
                new Canvas {
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = Guid.Parse(userId),
                    CreatedOn = DateTime.UtcNow,
                    Name = user.FriendlyName,
                    Objects = new List<CanvasObject>
                {
                    new() { Id = 3, name = "Object 1" },
                    new() { Id = 4, name = "Object 2" },
                }},
                new Canvas {
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow,
                    Name = user.FriendlyName,
                    Objects = new List<CanvasObject>
                {
                    new() { Id = 5, name = "Object 1" },
                    new() { Id = 6, name = "Object 2" },
                }} // Canvas with different user
            };

            _context.Canvases.AddRange(canvasList);
            _context.SaveChanges();

            var expectedResult = canvasList.Take(2).ToList(); // Expect only canvases of the user

            // Act
            var resultTask = _service.GetCanvasProjects(user, 1, 2);
            var result = await resultTask;
            var resultAsList = await result.ToListAsync();

            resultAsList.Should().BeEquivalentTo(expectedResult);
        }

        // Test for method GetTotalPages
        [Test]
        public async Task GetTotalPages_ShouldReturnCorrectTotalPages()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;

            _context.ApplicationUsers.Add(user);
            _context.SaveChanges();

            var canvasList = new List<Canvas>
    {
        new Canvas {
            Id = Guid.NewGuid(),
            User = user,
            UserId = Guid.Parse(userId),
            CreatedOn = DateTime.UtcNow,
            Name = user.FriendlyName,
            Objects = new List<CanvasObject>
        {
            new() { Id = 1, name = "Object 1" },
            new() { Id = 2, name = "Object 2" },
        }},
        new Canvas {
            Id = Guid.NewGuid(),
            User = user,
            UserId = Guid.Parse(userId),
            CreatedOn = DateTime.UtcNow,
            Name = user.FriendlyName,
            Objects = new List<CanvasObject>
        {
            new() { Id = 3, name = "Object 1" },
            new() { Id = 4, name = "Object 2" },
        }},
        new Canvas {
            Id = Guid.NewGuid(),
            User = user,
            UserId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            Name = user.FriendlyName,
            Objects = new List<CanvasObject>
        {
            new() { Id = 5, name = "Object 1" },
            new() { Id = 6, name = "Object 2" },
        }} // Canvas with different user
    };

            _context.Canvases.AddRange(canvasList);
            _context.SaveChanges();

            int expectedResult = 1; // Total canvases for the user

            // Act
            var result = await _service.GetTotalPages(user, 8);

            // Assert
            result.Should().Be(expectedResult);
        }

        // Test for method GetCanvasDetails
        [Test]
        public async Task GetCanvasDetails_ShouldReturnCanvasViewModel_WhenCanvasExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;

            _context.ApplicationUsers.Add(user);
            _context.SaveChanges();

            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasDetails(canvas.Id, userId);

            // Add categories to the result
            var categories = new List<CategoryViewModel>
            {
                new CategoryViewModel { Id = 1, Name = Enum.GetName(typeof(CategoryType), CategoryType.Buses) },
                new CategoryViewModel { Id = 2, Name = Enum.GetName(typeof(CategoryType), CategoryType.Cars) }
            };
            result.Categories = categories;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CanvasViewModel>();
            result.Id.Should().Be(canvas.Id);
            result.TotalPrice.Should().Be(0); // No prices set in the test canvas
            result.Thumbnail.Should().BeNull(); // No Thumbnail set in the test canvas
            result.Canvas.Name.Should().Be(canvas.Name);
            result.Canvas.Objects.Should().BeEquivalentTo(canvas.Objects);
            result.Categories.Should().BeEquivalentTo(categories);

        }

        // Test for method GetCanvasDetails when Canvas is not existing
        [Test]
        public async Task GetCanvasDetails_ShouldReturnNull_WhenCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.GetCanvasDetails(canvasId, userId);

            // Assert
            result.Should().BeNull();
        }

        // Test for method CreateCanvas when valid user and Canvas
        [Test]
        public async Task CreateCanvas_ShouldReturnTrue_WhenValidUserAndCanvas()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser { Id = userId };
            var canvas = new Canvas { Name = "Test Canvas", Description = "Test Description" };

            // Act
            var result = await _service.CreateCanvas(user, canvas);

            // Assert
            result.Should().BeTrue();
            _context.Canvases.Should().HaveCount(1);
            _context.Canvases.First().UserId.ToString().Should().Be(userId);
            _context.Canvases.First().Name.Should().Be("Test Canvas");
            _context.Canvases.First().Description.Should().Be("Test Description");
        }

        // Test for method CreateCanvas when invalid user
        [Test]
        public async Task CreateCanvas_ShouldReturnFalse_WhenInvalidUser()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser { Id = "123" };
            var canvas = new Canvas { Name = "Test Canvas", Description = "Test Description" };

            // Act
            var result = await _service.CreateCanvas(user, canvas);

            // Assert
            result.Should().BeFalse();
        }

        // Test for method GetCanvasForEdit when Canvas is not existing
        [Test]
        public async Task GetCanvasForEdit_ShouldReturnCorrectCanvas_WhenCanvasExists()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var canvasId = canvas.Id;

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForEdit(canvasId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Canvas>();
            result.Id.Should().Be(canvasId);
        }

        // Test for method GetCanvasForEdit when invalid user
        [Test]
        public async Task GetCanvasForEdit_ShouldReturnFalse_WhenInvalidUser()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(user.Id),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = Guid.NewGuid().ToString();
            var canvasId = canvas.Id;

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForEdit(canvasId, userId);

            // Assert
            result.Should().BeNull();
        }

        // Test for method GetCanvasForEdit when Canvas is not existing
        [Test]
        public async Task GetCanvasForEdit_ShouldReturnFalse_WhenInvalidCanvas()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(user.Id),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = Guid.NewGuid().ToString();
            var canvasId = Guid.NewGuid();

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForEdit(canvasId, userId);

            // Assert
            result.Should().BeNull();
        }

        // Test for method EditCanvas when valid Canvas and UserId
        [Test]
        public async Task EditCanvas_ShouldReturnTrue_WhenValidCanvasAndUserId()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var canvas = new Canvas { Id = canvasId, UserId = Guid.Parse(userId), Name = "Old Name" };
            var newCanvas = new Canvas { Id = canvasId, UserId = Guid.Parse(userId), Name = "New Name" };

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.EditCanvas(canvasId, userId, newCanvas);

            // Assert
            result.Should().BeTrue();
            _context.Canvases.First().Name.Should().Be("New Name");
        }

        // Test for method EditCanvas when invalid userId
        [Test]
        public async Task EditCanvas_ShouldReturnFalse_WhenInvalidUserId()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = user.UserId,
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = Guid.NewGuid().ToString();
            var canvasId = Guid.NewGuid();

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.EditCanvas(canvasId, userId, canvas);

            // Assert
            result.Should().BeFalse();
        }

        // Test for method EditCanvas when invalid Canvas
        [Test]
        public async Task EditCanvas_ShouldReturnFalse_WhenCanvasIsNotExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = user.UserId,
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = Guid.NewGuid().ToString();
            var canvasId = Guid.NewGuid();

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.EditCanvas(canvas.Id, userId, canvas);

            // Assert
            result.Should().BeFalse();
        }

        // Test for method GetCanvasForDelete when valid Canvas and UserId
        [Test]
        public async Task GetCanvasForDelete_ShouldReturnCorrectCanvas_WhenCanvasExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(user.Id),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = user.Id;
            var canvasId = canvas.Id;

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForDelete(canvasId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Canvas>();
            result.Id.Should().Be(canvasId);
        }

        // Test for method GetCanvasForDelete when invalid User
        [Test]
        public async Task GetCanvasForDelete_ShouldReturnFalse_WhenInvalidUser()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.NewGuid(),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = user.Id;
            var canvasId = canvas.Id;

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForDelete(canvasId, userId);

            // Assert
            result.Should().BeNull();
        }

        // Test for method GetCanvasForDelete when invalid Canvas
        [Test]
        public async Task GetCanvasForDelete_ShouldReturnFalse_WhenInvalidCanvas()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = user.UserId,
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var userId = user.Id;
            var canvasId = Guid.NewGuid();

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCanvasForDelete(canvasId, userId);

            // Assert
            result.Should().BeNull();
        }

        // Test for method DeleteCanvas when valid Canvas and UserId
        [Test]
        public async Task DeleteCanvas_ShouldReturnTrue_WhenValidCanvasAndUserId()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var canvas = new Canvas { Id = canvasId, UserId = Guid.Parse(userId), Name = "Test Canvas" };

            var baseObject = new BaseObject { Id = 1 };

            canvas.BaseObject = baseObject;

            _context.BaseObjects.Add(baseObject);
            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.DeleteCanvas(canvasId, userId);

            // Assert
            result.Should().BeTrue();
            _context.Canvases.Should().BeEmpty();
        }

        // Test for method DeleteCanvas when invalid Canvas
        [Test]
        public async Task DeleteCanvas_ShouldReturnFalse_WhenInvalidCanvas()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var canvas = new Canvas { Id = canvasId, UserId = Guid.Parse(userId), Name = "Test Canvas" };

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.DeleteCanvas(Guid.NewGuid(), userId);

            // Assert
            result.Should().BeFalse();
        }

        // Test for method CanvasExists
        [Test]
        public async Task CanvasExists_ShouldReturnTrue_WhenCanvasExists()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var canvas = new Canvas { Id = canvasId, UserId = Guid.Parse(userId), Name = "Test Canvas" };

            _context.Canvases.Add(canvas);
            _context.SaveChanges();

            // Act
            var result = await _service.CanvasExists(canvasId);

            // Assert
            result.Should().BeTrue();
        }

        // Test for method CanvasExists when Canvas does not exist
        [Test]
        public async Task CanvasExists_ShouldReturnFalse_WhenCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();

            // Act
            var result = await _service.CanvasExists(canvasId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
