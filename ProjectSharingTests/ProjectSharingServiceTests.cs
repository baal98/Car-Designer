using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ProjectSharingTests
{
    [TestFixture]

    public class ProjectSharingServiceTests
    {
        private ApplicationDbContext _context;
        private ProjectSharingService _service;
        private Guid projectId;
        private Guid sharingUserId;
        private Guid userId;
        private string thumbnail;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new ProjectSharingService(_context);

            // Generate random Guids for projectId and sharingUserId
            projectId = Guid.NewGuid();
            sharingUserId = Guid.NewGuid();
            userId = Guid.NewGuid();
            thumbnail = "thumbnail";
        }

        [Test]
        public async Task ShareProjectAsync_ShouldAddSharedProject_WhenValidDataIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                var projectId = Guid.NewGuid();
                var userId = "user-id";
                var thumbnail = "thumbnail";

                // Add a sample canvas to the database
                var canvas = new Canvas
                {
                    Id = projectId,
                    UserId = Guid.Parse(userId),
                    Name = "Sample Canvas",
                    Thumbnail = thumbnail,
                    Description = "Sample Canvas Description",
                };

                context.Canvases.Add(canvas);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                // Act
                await service.ShareProjectAsync(projectId, userId.ToString(), thumbnail);

                // Assert
                var sharedProjects = await context.SharedProjects.ToListAsync();
                sharedProjects.Should().ContainSingle();
                sharedProjects[0].SharingUserId.Should().Be(Guid.Parse(userId.ToString()));
                sharedProjects[0].CanvasId.Should().Be(projectId);
                sharedProjects[0].IsActive.Should().BeTrue();
            }
        }

        [Test]
        public async Task AddToCollectionAsync_ShouldAddClonedProject_WhenValidDataIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                var projectId = Guid.NewGuid();
                var userId = "user-id";

                // Add a sample shared project to the database
                var sharedProject = new SharedProject
                {
                    CanvasId = projectId,
                    SharingUserId = Guid.NewGuid(),
                    IsActive = true,
                    Canvas = new Canvas
                    {
                        Id = projectId,
                        UserId = Guid.NewGuid(),
                        Name = "Sample Canvas",
                        Thumbnail = "thumbnail",
                        Description = "Sample Canvas Description",
                    }
                };

                context.SharedProjects.Add(sharedProject);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                // Act
                var result = await service.AddToCollectionAsync(projectId, userId.ToString());

                // Assert
                result.Should().Be("Project successfully added to your collection!");

                var clonedProjects = await context.Canvases.Where(c => c.UserId == Guid.Parse(userId.ToString())).ToListAsync();
                clonedProjects.Should().ContainSingle();
                clonedProjects[0].Name.Should().Be("Sample Canvas");
                clonedProjects[0].Description.Should().Be("Sample Canvas Description");
            }
        }

        [Test]
        public async Task GetSharedProjectsAsync_ShouldReturnListOfSharedProjects_WhenValidUserIdIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                var userId = "user-id";

                // Add sample shared projects to the database
                var sharedProjects = new List<SharedProject>
                {
                    new SharedProject
                    {
                        CanvasId = Guid.NewGuid(),
                        SharingUserId = Guid.NewGuid(),
                        IsActive = true,
                        Canvas = new Canvas
                        {
                            Name = "Sample Canvas 1",
                            Description = "Sample Canvas Description 1",
                        }
                    },
                    new SharedProject
                    {
                        CanvasId = Guid.NewGuid(),
                        SharingUserId = Guid.NewGuid(),
                        IsActive = true,
                        Canvas = new Canvas
                        {
                            Name = "Sample Canvas 2",
                            Description = "Sample Canvas Description 2",
                        }
                    }
                };

                context.SharedProjects.AddRange(sharedProjects);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                // Act
                var result = await service.GetSharedProjectsAsync(userId.ToString());

                // Assert
                result.Should().HaveCount(2);
                result[0].CanvasName.Should().Be("Sample Canvas 1");
                result[0].Description.Should().Be("Sample Canvas Description 1");
                result[1].CanvasName.Should().Be("Sample Canvas 2");
                result[1].Description.Should().Be("Sample Canvas Description 2");
            }
        }

        [Test]
        public async Task DeleteProjectAsync_ShouldDeleteSharedAndClonedProjects_WhenValidProjectIdIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                var projectId = Guid.NewGuid();
                var sharingUserId = Guid.NewGuid();
                var collectingUserId = Guid.NewGuid();

                // Add a sample shared and cloned project to the database
                var sharedProject = new SharedProject
                {
                    CanvasId = projectId,
                    SharingUserId = sharingUserId,
                    IsActive = true,
                    Canvas = new Canvas
                    {
                        Id = projectId,
                        UserId = sharingUserId,
                        Name = "Sample Canvas",
                        Thumbnail = "thumbnail",
                        Description = "Sample Canvas Description",
                    }
                };

                var clonedProject = new Canvas
                {
                    Id = projectId,
                    UserId = collectingUserId,
                    Name = "Sample Canvas",
                    Thumbnail = "thumbnail",
                    Description = "Sample Canvas Description",
                };

                context.SharedProjects.Add(sharedProject);
                context.Canvases.Add(clonedProject);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                // Act
                await service.DeleteProjectAsync(projectId);

                // Assert
                var sharedProjects = await context.SharedProjects.ToListAsync();
                sharedProjects.Should().BeEmpty();

                var clonedProjects = await context.Canvases.ToListAsync();
                clonedProjects.Should().BeEmpty();
            }
        }


        [Test]
        public async Task GetProjectDetails_ShouldReturnSharedProjectViewModel_WhenValidProjectIdIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                var projectId = Guid.NewGuid();
                var sharingUserId = Guid.NewGuid();

                // Add a sample shared project to the database
                var sharedProject = new SharedProject
                {
                    CanvasId = projectId,
                    SharingUserId = sharingUserId,
                    IsActive = true,
                    Canvas = new Canvas
                    {
                        Id = projectId,
                        UserId = sharingUserId,
                        Name = "Sample Canvas",
                        Thumbnail = "thumbnail",
                        Description = "Sample Canvas Description",
                        CreatedOn = DateTime.Now,
                    }
                };

                context.SharedProjects.Add(sharedProject);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProjectSharingService(context);

                // Act
                var result = await service.GetProjectDetails(projectId);

                // Assert
                result.Should().NotBeNull();
                result.CanvasId.Should().Be(projectId);
                result.UserId.Should().Be(sharingUserId);
                result.CanvasName.Should().Be("Sample Canvas");
                result.Description.Should().Be("Sample Canvas Description");
                result.Thumbnail.Should().Be("thumbnail");
                result.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            }
        }



    }
}
