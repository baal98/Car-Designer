using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Web.ViewModels.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Service.Tests
{
    public class CanvasServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Canvas, CanvasDto>();
                cfg.CreateMap<CanvasDto, Canvas>();
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Test]
        public async Task GetCanvasesAsync_ReturnsAllCanvases()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas1" });
                context.Canvases.Add(new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas2" });
                await context.SaveChangesAsync();
            }

            // Act
            IEnumerable<Canvas> canvases;
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                canvases = await service.GetCanvasesAsync();
            }

            // Assert
            Assert.AreEqual(2, canvases.Count());
        }

        [Test]
        public async Task GetCanvasAsync_ReturnsCanvasIfExists()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(new Canvas { Id = canvasId, Name = "Test Canvas1" });
                await context.SaveChangesAsync();
            }

            // Act
            CanvasDto result;
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                result = await service.GetCanvasAsync(canvasId);
            }

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(canvasId, result.Id);
        }

        [Test]
        public async Task GetCanvasAsync_ReturnsNullIfCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();

            // Act
            CanvasDto result;
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                result = await service.GetCanvasAsync(canvasId);
            }

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task CreateCanvasAsync_CreatesCanvas()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;
            var baseObject = new BaseObject();
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                BaseObject = baseObject,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                var result = await service.CreateCanvasAsync(canvas);
            }

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var savedCanvas = await context.Canvases.FindAsync(canvas.Id);
                Assert.IsNotNull(savedCanvas);
                Assert.AreEqual(canvas.Name, savedCanvas.Name);
                Assert.AreEqual(canvas.Description, savedCanvas.Description);
            }
        }

        [Test]
        public async Task UpdateCanvasAsync_UpdatesCanvas()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;
            var baseObject = new BaseObject();
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                BaseObject = baseObject,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(canvas);
                await context.SaveChangesAsync();
            }

            var updatedCanvas = new Canvas { Id = canvas.Id, Name = "Updated Canvas", Description = "Updated Description" };

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                await service.UpdateCanvasAsync(canvas.Id, updatedCanvas);
            }

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var savedCanvas = await context.Canvases.FindAsync(canvas.Id);
                Assert.IsNotNull(savedCanvas);
                Assert.AreEqual(updatedCanvas.Name, savedCanvas.Name);
                Assert.AreEqual(updatedCanvas.Description, savedCanvas.Description);
            }
        }

        [Test]
        public void UpdateCanvasAsync_ThrowsExceptionIfCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            var canvas = new Canvas { Id = canvasId, Name = "Non-Existent Canvas", Description = "Does not exist" };

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                Assert.ThrowsAsync<Exception>(async () => await service.UpdateCanvasAsync(canvasId, canvas));
            }
        }

        [Test]
        public async Task UpdateCanvasAsync_UpdatesBaseObjectIfNotNull()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", FriendlyName = "Test User" };
            var userId = user.Id;
            var baseObject = new BaseObject{Id = 5};
            var canvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
                BaseObject = baseObject,
                Objects = new List<CanvasObject>
                {
                    new() { Id = 1, name = "Object 1" },
                    new() { Id = 2, name = "Object 2" },
                }
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var updatedCanvas = new Canvas
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = Guid.Parse(userId),
                CreatedOn = DateTime.UtcNow,
                Name = user.FriendlyName,
            };
            using (var context = new ApplicationDbContext(options))
            {
                context.Canvases.Add(canvas);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var service = new CanvasService(context, _mapper);
                await service.UpdateCanvasAsync(canvas.Id, updatedCanvas);
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var savedCanvas = await context.Canvases.FindAsync(canvas.Id);
                Assert.IsNull(savedCanvas.BaseObject);
                Assert.AreEqual(updatedCanvas.Name, savedCanvas.Name);
                Assert.AreEqual(updatedCanvas.Description, savedCanvas.Description);
            }

            // Clear InMemory database
            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureDeleted();
            }
        }


        [Test]
        public async Task UpdateCanvasAsync_UpdatesAndAddsObjects()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test@test.com", FriendlyName = "Test User" };
            var objects = new List<CanvasObject> { new CanvasObject { Id = 1, name = "Object1" }, new CanvasObject { Id = 2, name = "Object2" } };
            var baseObject1 = new BaseObject { Id = 7 };
            var canvas = new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas1", User = user, UserId = Guid.Parse(user.Id), Objects = objects, Thumbnail = "123", BaseObject = baseObject1};
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(canvas);
                await context.SaveChangesAsync();
            }

            var baseObject2 = new BaseObject { Id = 6 };
            var updatedObjects = new List<CanvasObject> { new CanvasObject { Id = 3, name = "Object1Updated" }, new CanvasObject { Id = 4, name = "Object3" } };
            var updatedCanvas = new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas2", Objects = updatedObjects, BaseObject = baseObject2};

            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(updatedCanvas);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                await service.UpdateCanvasAsync(canvas.Id, updatedCanvas);
            }

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var savedCanvas = await context.Canvases.Include(c => c.Objects).FirstOrDefaultAsync(c => c.Id == canvas.Id);
                Assert.IsNotNull(savedCanvas.Objects);
                Assert.AreEqual(2, savedCanvas.Objects.Count); // Two original, one updated and one new
                Assert.AreEqual(updatedObjects[0].name, savedCanvas.Objects.First(o => o.Id == updatedObjects[0].Id).name);
                Assert.IsNotNull(savedCanvas.Objects.FirstOrDefault(o => o.Id == updatedObjects[1].Id));
            }
        }

        [Test]
        public async Task UpdateCanvasAsync_UpdatesThumbnailIfNotNull()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test@test.com", FriendlyName = "Test User" };
            var objects = new List<CanvasObject> { new CanvasObject { Id = 1, name = "Object1" }, new CanvasObject { Id = 2, name = "Object2" } };
            var baseObject1 = new BaseObject { Id = 7 };
            var canvas = new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas1", User = user, UserId = Guid.Parse(user.Id), Objects = objects, Thumbnail = "123", BaseObject = baseObject1 };
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(canvas);
                await context.SaveChangesAsync();
            }

            var baseObject2 = new BaseObject { Id = 6 };
            var updatedObjects = new List<CanvasObject> { new CanvasObject { Id = 3, name = "Object1Updated" }, new CanvasObject { Id = 4, name = "Object3" } };
            var updatedCanvas = new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas2", Objects = updatedObjects, Thumbnail = "456", BaseObject = baseObject2 };

            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(updatedCanvas);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                await service.UpdateCanvasAsync(canvas.Id, updatedCanvas);
            }

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var savedCanvas = await context.Canvases.Include(c => c.Objects).FirstOrDefaultAsync(c => c.Id == canvas.Id);
                Assert.IsNotNull(savedCanvas.Objects);
                Assert.AreEqual(2, savedCanvas.Objects.Count); // Two original, one updated and one new
                Assert.AreEqual(updatedObjects[0].name, savedCanvas.Objects.First(o => o.Id == updatedObjects[0].Id).name);
                Assert.IsNotNull(savedCanvas.Objects.FirstOrDefault(o => o.Id == updatedObjects[1].Id));
                Assert.AreEqual(savedCanvas.Thumbnail, updatedCanvas.Thumbnail);
            }
        }


        [Test]
        public async Task DeleteCanvasAsync_DeletesCanvas()
        {
            // Arrange
            var canvas = new Canvas { Id = Guid.NewGuid(), Name = "Test Canvas" };
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(canvas);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                await service.DeleteCanvasAsync(canvas.Id);
            }

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var deletedCanvas = await context.Canvases.FindAsync(canvas.Id);
                Assert.IsNull(deletedCanvas);
            }
        }

        [Test]
        public async Task DeleteCanvasAsync_ThrowsExceptionIfCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();

            // Act
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                Assert.ThrowsAsync<Exception>(async () => await service.DeleteCanvasAsync(canvasId));
            }
        }

        [Test]
        public void CanvasExists_ReturnsCorrectResult()
        {
            // Arrange
            var canvasId = Guid.NewGuid();
            using (var context = new ApplicationDbContext(_options))
            {
                context.Canvases.Add(new Canvas { Id = canvasId, Name = "Test Canvas" });
                context.SaveChanges();
            }

            // Act
            bool exists;
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                exists = service.CanvasExists(canvasId);
            }

            // Assert
            Assert.IsTrue(exists);
        }

        [Test]
        public void CanvasExists_ReturnsFalseIfCanvasDoesNotExist()
        {
            // Arrange
            var canvasId = Guid.NewGuid();

            // Act
            bool exists;
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CanvasService(context, _mapper);
                exists = service.CanvasExists(canvasId);
            }

            // Assert
            Assert.IsFalse(exists);
        }
    }
}
