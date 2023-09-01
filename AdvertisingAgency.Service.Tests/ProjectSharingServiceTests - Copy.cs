//using AdvertisingAgency.Data.Data;
//using AdvertisingAgency.Data.Data.Models;
//using AdvertisingAgency.Services;
//using AdvertisingAgency.Services.Interfaces;
//using FluentAssertions;
//using Microsoft.CodeAnalysis;
//using Microsoft.EntityFrameworkCore;

//namespace AdvertisingAgency.Service.Tests
//{
//    [TestFixture]
//    public class ProjectSharingServiceTests
//    {
//        private ApplicationDbContext _context;
//        private Guid _projectId;
//        private Guid _userId;
//        private string _thumbnail;
//        private IProjectSharingService service;

//        [SetUp]
//        public void SetUp()
//        {
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            _context = new ApplicationDbContext(options);
//            _projectId = Guid.NewGuid();
//            _userId = Guid.NewGuid();
//            _thumbnail = "thumbnail";
//            service = new ProjectSharingService(_context);
//        }

//        [Test]
//        public async Task ShareProjectAsync_ShouldAddSharedProject_WhenValidDataIsProvided()
//        {
//            // Arrange
//            // Add a sample canvas to the database
//            var canvas = new Canvas
//            {
//                Id = _projectId,
//                UserId = _userId,
//                Name = "Sample Canvas",
//                Thumbnail = _thumbnail,
//                Description = "Sample Canvas Description",
//            };

//            _context.Canvases.Add(canvas);
//            _context.SaveChanges();

//            // Act
//            await service.ShareProjectAsync(_projectId, _userId.ToString(), _thumbnail);

//            // Assert
//            var sharedProjects = await _context.SharedProjects.ToListAsync();
//            sharedProjects.Should().ContainSingle();
//            sharedProjects[0].SharingUserId.Should().Be(_userId);
//            sharedProjects[0].CanvasId.Should().Be(_projectId);
//            sharedProjects[0].IsActive.Should().BeTrue();
//        }

//        [Test]
//        public async Task ShareProjectAsync_ShouldThrowArgumentException_WhenProjectIdIsInvalid()
//        {
//            // Arrange
//            var invalidProjectId = Guid.NewGuid(); // A random invalid project ID

//            // Act & Assert
//            Assert.ThrowsAsync<ArgumentException>(async () => await service.ShareProjectAsync(invalidProjectId, _userId.ToString(), _thumbnail));
//        }

//        [Test]
//        public async Task ShareProjectAsync_ShouldThrowCustomException_WhenProjectWithSameNameIsAlreadySharedBySameAuthor()
//        {
//            // Arrange
//            // Add a sample canvas to the database
//            var canvas = new Canvas
//            {
//                Id = _projectId,
//                UserId = _userId,
//                Name = "Sample Canvas",
//                Thumbnail = _thumbnail,
//                Description = "Sample Canvas Description",
//            };

//            _context.Canvases.Add(canvas);
//            _context.SaveChanges();

//            // Share the project for the first time
//            await service.ShareProjectAsync(_projectId, _userId.ToString(), _thumbnail);

//            // Act & Assert (attempt to share the project again with the same data)
//            Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.ShareProjectAsync(_projectId, _userId.ToString(), _thumbnail));
//        }

//        [Test]
//        public async Task ShareProjectAsync_ShouldCloneObjects_WhenObjectsExistInCanvas()
//        {
//            // Arrange
//            // Add a sample canvas to the database with some objects
//            var canvas = new Canvas
//            {
//                Id = _projectId,
//                UserId = _userId,
//                Name = "Sample Canvas",
//                Thumbnail = _thumbnail,
//                Description = "Sample Canvas Description",
//                Objects = new List<CanvasObject>
//                {
//                    new() { Id = 1, name = "Object 1" },
//                    new() { Id = 2, name = "Object 2" },
//                }
//            };

//            _context.Canvases.Add(canvas);
//            _context.SaveChanges();

//            // Act
//            await service.ShareProjectAsync(_projectId, _userId.ToString(), _thumbnail);

//            // Assert
//            var newCanvas = await _context.Canvases
//                .Include(c => c.Objects)
//                .FirstOrDefaultAsync(c => c.UserId == _userId && c.Name == "Sample Canvas");

//            Assert.IsNotNull(newCanvas); // Ensure the new canvas exists
//            Assert.AreEqual(2, newCanvas.Objects.Count); // Ensure the objects are cloned and added to the new canvas
//        }

//        [Test]
//        public async Task ShareProjectAsync_ShouldCloneBaseObject_WhenBaseObjectExistsInCanvas()
//        {
//            // Arrange
//            // Add a sample canvas to the database with a base object
//            var canvas = new Canvas
//            {
//                Id = _projectId,
//                UserId = _userId,
//                Name = "Sample Canvas",
//                Thumbnail = _thumbnail,
//                Description = "Sample Canvas Description",
//                BaseObject = new BaseObject { Id = 42, }
//            };

//            _context.Canvases.Add(canvas);
//            _context.SaveChanges();

//            // Act
//            await service.ShareProjectAsync(_projectId, _userId.ToString(), _thumbnail);

//            // Assert
//            var newCanvas = await _context.Canvases
//                .Include(c => c.BaseObject)
//                .FirstOrDefaultAsync(c => c.UserId == _userId && c.Name == "Sample Canvas");

//            Assert.IsNotNull(newCanvas); // Ensure the new canvas exists
//            Assert.IsNotNull(newCanvas.BaseObject); // Ensure the base object is cloned and assigned to the new canvas
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldAddClonedProject_WhenValidDataIsProvided()
//        {
//            // Arrange
//            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
//            var userToGet = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };

//            // Add the users to the database
//            _context.ApplicationUsers.Add(user);
//            _context.ApplicationUsers.Add(userToGet);

//            var projectId = Guid.NewGuid();

//            // Add a sample shared project to the database
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = user.UserId,
//                CollectingUserId = userToGet.UserId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = Guid.Parse(user.Id),
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.SaveChanges();

//            // Act
//            var result = await service.AddToCollectionAsync(projectId, userToGet.Id);

//            // Assert
//            result.Should().Be("Project successfully added to your collection!");

//            var clonedProjects = await _context.Canvases.Where(c => c.UserId.ToString() == user.Id).ToListAsync();
//            clonedProjects.Should().ContainSingle();
//            clonedProjects[0].Name.Should().Be("Sample Canvas");
//            clonedProjects[0].Description.Should().Be("Sample Canvas Description");
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldThrowException_WhenCanvasIdDoesNotExist()
//        {
//            // Arrange

//            // Act & Assert
//            Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.AddToCollectionAsync(Guid.NewGuid(), "user-id"));
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldCloneObjects_WhenObjectsExist()
//        {
//            // Arrange

//            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user1@test.com", UserName = "User 1" };
//            var user2 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user2@test.com", UserName = "User 2" };

//            _context.ApplicationUsers.Add(user1);
//            _context.ApplicationUsers.Add(user2);

//            var userId = Guid.NewGuid();
//            var projectId = Guid.NewGuid();

//            // Add a sample shared project to the database with objects
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = Guid.NewGuid(),
//                SharingUser = user1,
//                CollectingUserId = userId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = Guid.NewGuid(),
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                    Objects = new List<CanvasObject>
//                    {
//                        new () { Id = 1, },
//                        new () { Id = 2, }
//                    }
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.SaveChanges();

//            // Act
//            await service.AddToCollectionAsync(projectId, user2.Id);

//            // Assert
//            var clonedObjects = await _context.objects.Where(obj => obj.Id != sharedProject.Canvas.Objects[0].Id && obj.Id != sharedProject.Canvas.Objects[1].Id).ToListAsync();
//            Assert.AreEqual(sharedProject.Canvas.Objects.Count, clonedObjects.Count);
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldThrowException_WhenUserOwnsProjectWithSameName()
//        {
//            // Arrange
//            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user1@test.com", UserName = "User 1" };

//            _context.ApplicationUsers.Add(user1);

//            var projectId = Guid.NewGuid();

//            // Add a sample shared project to the database, created by user1
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = Guid.NewGuid(),
//                SharingUser = user1,
//                CollectingUserId = Guid.NewGuid(), // Another user as the collecting user
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = Guid.NewGuid(), // Another user as the creator of the project
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);

//            // Add the same project to the database but created by user1
//            var clonedProject = new Canvas
//            {
//                Id = Guid.NewGuid(),
//                UserId = Guid.Parse(user1.Id),
//                Name = "Sample Canvas", // Using the same name as sharedProject.Canvas.Name
//                Thumbnail = "thumbnail",
//                Description = "Sample Canvas Description",
//            };

//            _context.Canvases.Add(clonedProject);
//            _context.SaveChanges();

//            // Act & Assert
//            Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.AddToCollectionAsync(projectId, user1.Id));
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldThrowException_WhenUserAlreadyOwnsProjectWithSameName1()
//        {
//            // Arrange
//            var projectId = Guid.NewGuid();
//            var userId = Guid.NewGuid();

//            // Add a sample shared project to the database
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = Guid.NewGuid(),
//                CollectingUserId = Guid.NewGuid(),
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = Guid.NewGuid(),
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.SaveChanges();

//            // Add a cloned project to the database with the same name
//            var clonedProject = new Canvas
//            {
//                Id = Guid.NewGuid(),
//                UserId = userId,
//                Name = "Sample Canvas",
//                Thumbnail = "thumbnail",
//                Description = "Sample Canvas Description",
//            };

//            _context.Canvases.Add(clonedProject);
//            _context.SaveChanges();

//            // Act & Assert
//            Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.AddToCollectionAsync(projectId, userId.ToString()));
//        }

//        [Test]
//        public async Task AddToCollectionAsync_ShouldThrowException_WhenUserDoesNotExist()
//        {
//            // Arrange
//            var projectId = Guid.NewGuid();

//            // Add a sample shared project to the database
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = Guid.NewGuid(),
//                CollectingUserId = Guid.NewGuid(),
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = Guid.NewGuid(),
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.SaveChanges();

//            // Act & Assert
//           Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.AddToCollectionAsync(projectId, Guid.NewGuid().ToString()));
//        }
        
//        [Test]
//        public async Task AddToCollectionAsync_ShouldThrowException_WhenProjectIdDoesNotExist()
//        {
//            // Arrange
//            var userId = Guid.NewGuid().ToString(); // Use a valid userId for testing

//            // Act & Assert
//            Assert.ThrowsAsync<ProjectSharingService.CustomException>(async () => await service.AddToCollectionAsync(Guid.NewGuid(), userId));
//        }


//        [Test]
//        public async Task GetSharedProjectsAsync_ShouldReturnListOfSharedProjects_WhenValidUserIdIsProvided()
//        {
//            // Arrange
//            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user1@test.com", UserName = "User 1" };
//            var user2 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user2@test.com", UserName = "User 2" };
//            var user3 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user3@test.com", UserName = "User 3" };

//            // Add the users to the database
//            _context.ApplicationUsers.Add(user1);
//            _context.ApplicationUsers.Add(user2);
//            _context.ApplicationUsers.Add(user3);

//            var projectId1 = Guid.NewGuid();
//            var projectId2 = Guid.NewGuid();

//            // Add shared projects to the database
//            var sharedProject1 = new SharedProject
//            {
//                CanvasId = projectId1,
//                SharingUserId = user1.UserId,
//                CollectingUserId = user2.UserId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId1,
//                    UserId = Guid.Parse(user1.Id),
//                    Name = "Sample Canvas 1",
//                    Thumbnail = "thumbnail1",
//                    Description = "Sample Canvas Description 1",
//                }
//            };

//            var sharedProject2 = new SharedProject
//            {
//                CanvasId = projectId2,
//                SharingUserId = user2.UserId,
//                CollectingUserId = user1.UserId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId2,
//                    UserId = Guid.Parse(user2.Id),
//                    Name = "Sample Canvas 2",
//                    Thumbnail = "thumbnail2",
//                    Description = "Sample Canvas Description 2",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject1);
//            _context.SharedProjects.Add(sharedProject2);
//            _context.SaveChanges();

//            // Act
//            var result = await service.GetSharedProjectsAsync(user1.Id);

//            // Assert
//            result.Should().HaveCount(2);

//            // First shared project (result[0])
//            result[0].CanvasId.Should().Be(projectId2);
//            result[0].UserId.Should().Be(user2.UserId);
//            result[0].CanvasName.Should().Be("Sample Canvas 2");
//            result[0].Description.Should().Be("Sample Canvas Description 2");
//            result[0].Thumbnail.Should().Be("thumbnail2");
//            result[0].Username.Should().Be(user2.FriendlyName);

//            // Second shared project (result[1])
//            result[1].CanvasId.Should().Be(projectId1);
//            result[1].UserId.Should().Be(user1.UserId);
//            result[1].CanvasName.Should().Be("Sample Canvas 1");
//            result[1].Description.Should().Be("Sample Canvas Description 1");
//            result[1].Thumbnail.Should().Be("thumbnail1");
//            result[1].Username.Should().Be(user1.FriendlyName);
//        }

//        [Test]
//        public async Task DeleteProjectAsync_ShouldDeleteSharedAndOriginalProjects_WhenValidProjectIdIsProvided()
//        {
//            // Arrange
//            var sharingUserId = Guid.NewGuid();
//            var collectingUserId = Guid.NewGuid();
//            var projectId = Guid.NewGuid();

//            // Add a sample shared and cloned project to the database
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = sharingUserId,
//                CollectingUserId = collectingUserId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = sharingUserId,
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.Canvases.Add(sharedProject.Canvas); // adding original project to the DB
//            _context.SaveChanges();

//            // Act
//            await service.DeleteProjectAsync(projectId);

//            // Assert
//            var sharedProjects = await _context.SharedProjects.ToListAsync();
//            sharedProjects.Should().BeEmpty();

//            var clonedProjects = await _context.Canvases.ToListAsync();
//            clonedProjects.Should().BeEmpty();
//        }

//        [Test]
//        public async Task DeleteProjectAsync_ShouldThrowException_WhenSharedProjectNotFound()
//        {
//            // Arrange
//            var projectId = Guid.NewGuid();

//            // Act and Assert
//            Assert.ThrowsAsync<Exception>(async () => await service.DeleteProjectAsync(projectId));
//        }


//        [Test]
//        public async Task GetProjectDetails_ShouldReturnSharedProjectViewModel_WhenValidProjectIdIsProvided()
//        {
//            // Arrange
//            var projectId = Guid.NewGuid();
//            var sharingUserId = Guid.NewGuid();

//            // Add a sample shared project to the database
//            var sharedProject = new SharedProject
//            {
//                CanvasId = projectId,
//                SharingUserId = sharingUserId,
//                IsActive = true,
//                Canvas = new Canvas
//                {
//                    Id = projectId,
//                    UserId = sharingUserId,
//                    Name = "Sample Canvas",
//                    Thumbnail = "thumbnail",
//                    Description = "Sample Canvas Description",
//                    CreatedOn = DateTime.Now,
//                }
//            };

//            _context.SharedProjects.Add(sharedProject);
//            _context.SaveChanges();

//            // Act
//            var result = await service.GetProjectDetails(projectId);

//            // Assert
//            result.Should().NotBeNull();
//            result.CanvasId.Should().Be(projectId);
//            result.UserId.Should().Be(sharingUserId);
//            result.CanvasName.Should().Be("Sample Canvas");
//            result.Description.Should().Be("Sample Canvas Description");
//            result.Thumbnail.Should().Be("thumbnail");
//            result.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
//        }
//    }
//}
