using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Data.Models.ProjectSharing;
using AdvertisingAgency.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    public class ProjectSharingService : IProjectSharingService
    {
        /// <summary>
        /// Exception raised for custom scenarios in the ProjectSharingService.
        /// </summary>
        public class CustomException : Exception
        {
            public CustomException(string message)
                : base(message)
            {
            }
        }

        private readonly ApplicationDbContext _context;

        public ProjectSharingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Shares a project by creating a copy of it and adding it to the shared projects.
        /// </summary>
        /// <param name="projectId">The ID of the project to be shared.</param>
        /// <param name="userId">The ID of the user sharing the project.</param>
        /// <param name="thumbnail">Thumbnail associated with the project.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ShareProjectAsync(Guid projectId, string userId, string thumbnail)
        {
            var canvas = await _context.Canvases
                                       .Include(c => c.Objects)
                                       .Include(c => c.BaseObject)
                                       .OrderByDescending(c => c.CreatedOn)
                                       .FirstOrDefaultAsync(c => c.Id == projectId);

            if (canvas == null)
            {
                throw new ArgumentException("Invalid project ID.");
            }

            // Check if the project has already been shared with the same name by the same author
            var isSharedProjectExists = await _context.SharedProjects.AnyAsync(sp =>
                sp.Canvas.Name == canvas.Name &&
                sp.SharingUserId == canvas.UserId && sp.Canvas.Thumbnail == thumbnail);

            if (isSharedProjectExists)
            {
                throw new CustomException("You have already shared a project with the same name!");
            }

            var sharedProject = new SharedProject
            {
                SharingUserId = Guid.Parse(userId),
                CanvasId = canvas.Id,
                IsActive = true
            };

            _context.SharedProjects.Add(sharedProject);

            var newCanvas = new Canvas
            {
                UserId = canvas.UserId,
                Name = canvas.Name,
                Thumbnail = canvas.Thumbnail,
                Description = canvas.Description
            };
            if (canvas.Objects != null)
            {
                foreach (var obj in canvas.Objects)
                {
                    var newObj = MapToNewCanvasObject(obj, newCanvas.Id);
                    newCanvas.Objects.Add(newObj);
                }
            }

            if (canvas.BaseObject != null)
            {
                var newBaseObject = MapToNewBaseObject(canvas.BaseObject, newCanvas.Id);
                newCanvas.BaseObject = newBaseObject;
            }

            _context.Canvases.Add(newCanvas);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a shared project to a user's collection.
        /// </summary>
        /// <param name="canvasId">The ID of the canvas of the shared project.</param>
        /// <param name="userId">The ID of the user adding the project to their collection.</param>
        /// <returns>A message indicating the success or failure of the operation.</returns>
        public async Task<string> AddToCollectionAsync(Guid canvasId, string userId)
        {
            // Find the shared project
            var sharedProject = await _context.SharedProjects
                .Include(sp => sp.Canvas)
                .ThenInclude(c => c.BaseObject)
                .Include(sp => sp.Canvas)
                .ThenInclude(c => c.Objects)
                .FirstOrDefaultAsync(sp => sp.CanvasId == canvasId && sp.IsActive.Value);

            if (sharedProject == null)
            {
                throw new CustomException("The project you are trying to collect does not exist.");
            }

            // Find the user
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new CustomException("User not found. Please check your login status.");
            }

            // Check if the user has already collected this project
            var userOwnsCanvas = await _context.Canvases.AnyAsync(c => c.UserId.ToString() == user.Id && c.Name == sharedProject.Canvas.Name);

            if (userOwnsCanvas)
            {
                throw new CustomException("You have already collected this project or you have a project with this name!");
            }

            // Clone the project
            var newCanvas = new Canvas
            {
                UserId = Guid.Parse(user.Id),
                Name = sharedProject.Canvas.Name,
                Thumbnail = sharedProject.Canvas.Thumbnail,
                Description = sharedProject.Canvas.Description,
                Objects = new List<CanvasObject>(),
            };

            _context.Canvases.Add(newCanvas);

            // Clone the objects
            if (sharedProject.Canvas.Objects != null)
            {
                var newObjects = sharedProject.Canvas.Objects
                    .Select(obj => MapToNewCanvasObject(obj, newCanvas.Id))
                    .ToList();

                _context.objects.AddRange(newObjects);
            }

            // Clone the base object
            if (sharedProject.Canvas.BaseObject != null)
            {
                var newBaseObject = MapToNewBaseObject(sharedProject.Canvas.BaseObject, newCanvas.Id);

                _context.BaseObjects.Add(newBaseObject);
            }

            // Save the changes to the database
            try
            {
                await _context.SaveChangesAsync();
                return "Project successfully added to your collection!";
            }
            catch (Exception)
            {
                throw new CustomException("An error occurred while trying to add the project to your collection. Please train again!");
            }
        }

        /// <summary>
        /// Retrieves a list of shared projects excluding those owned by the provided user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of shared projects.</returns>
        public async Task<List<SharedProjectViewModel>> GetSharedProjectsAsync(string userId)
        {
            var userGuid = new Guid(userId);

            var sharedProjects = await _context.SharedProjects
                .Include(sp => sp.Canvas)
                .Include(sp => sp.SharingUser)
                .Where(sp => sp.CollectingUserId != userGuid && sp.IsActive == true)
                .OrderByDescending(sp => sp.CreatedOn)
                .ToListAsync();

            var sharedProjectsViewModels = new List<SharedProjectViewModel>();

            foreach (var project in sharedProjects)
            {
                var userName = await _context.ApplicationUsers
                    .Where(u => u.Id == project.SharingUserId.ToString())
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                var projectViewModel = new SharedProjectViewModel
                {
                    CanvasId = project.CanvasId.GetValueOrDefault(),
                    UserId = project.SharingUserId.GetValueOrDefault(),
                    CanvasName = project.Canvas?.Name,
                    Description = project.Canvas?.Description,
                    Thumbnail = project.Canvas?.Thumbnail,
                    Username = userName,
                };

                sharedProjectsViewModels.Add(projectViewModel);
            }

            return sharedProjectsViewModels;
        }

        /// <summary>
        /// Deletes a shared project and its original if it exists.
        /// </summary>
        /// <param name="projectId">The ID of the project to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteProjectAsync(Guid projectId)
        {
            var sharedProject = await _context.SharedProjects.FirstOrDefaultAsync(sp => sp.CanvasId == projectId);
            
            if (sharedProject == null)
            {
                throw new Exception("Shared project not found");
            }

            // Намираме оригиналния проект в колекцията на потребителя
            var originalProject = await _context.Canvases.FirstOrDefaultAsync(c => c.Id == sharedProject.CanvasId && c.UserId == sharedProject.SharingUserId);

            // Ако оригиналния проект съществува, изтриваме го
            if (originalProject != null)
            {
                _context.Canvases.Remove(originalProject);
            }

            // Изтриваме споделения проект
            _context.SharedProjects.Remove(sharedProject);

            // Запазваме промените в базата данни
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the details of a shared project based on its ID.
        /// </summary>
        /// <param name="id">The ID of the shared project.</param>
        /// <returns>A ViewModel representing the details of the shared project.</returns>
        public async Task<SharedProjectViewModel> GetProjectDetails(Guid? id)
        {
            var sharedProject = await _context.SharedProjects
                .Where(sp => sp.CanvasId == id)
                .Select(sp => new SharedProjectViewModel
                {
                    CanvasId = sp.CanvasId.GetValueOrDefault(),
                    UserId = sp.SharingUserId.GetValueOrDefault(),
                    CanvasName = sp.Canvas.Name,
                    Description = sp.Canvas.Description,
                    Thumbnail = sp.Canvas.Thumbnail,
                    CreatedOn = sp.Canvas.CreatedOn.Value,
                })
                .FirstOrDefaultAsync();

            if (sharedProject != null)
            {
                var userName = await _context.ApplicationUsers
                    .Where(u => u.Id == sharedProject.UserId.ToString())
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                sharedProject.Username = userName;
            }

            return sharedProject;
        }

        /// <summary>
        /// Maps properties from an old CanvasObject to a new one.
        /// </summary>
        /// <param name="oldObj">The original CanvasObject.</param>
        /// <param name="newRootId">The ID for the new CanvasObject.</param>
        /// <returns>A new CanvasObject with properties copied from the old one.</returns>
        private CanvasObject MapToNewCanvasObject(CanvasObject oldObj, Guid newRootId)
        {
            var newObj = new CanvasObject
            {
                rootId = newRootId,
                Path = oldObj.Path,
                name = oldObj.name,
                type = oldObj.type,
                price = oldObj.price,
                version = oldObj.version,
                originX = oldObj.originX,
                originY = oldObj.originY,
                left = oldObj.left,
                top = oldObj.top,
                width = oldObj.width,
                height = oldObj.height,
                fill = oldObj.fill,
                stroke = oldObj.stroke,
                strokeWidth = oldObj.strokeWidth,
                strokeDashArray = oldObj.strokeDashArray,
                strokeLineCap = oldObj.strokeLineCap,
                strokeDashOffset = oldObj.strokeDashOffset,
                strokeLineJoin = oldObj.strokeLineJoin,
                strokeUniform = oldObj.strokeUniform,
                strokeMiterLimit = oldObj.strokeMiterLimit,
                scaleX = oldObj.scaleX,
                scaleY = oldObj.scaleY,
                angle = oldObj.angle,
                flipX = oldObj.flipX,
                flipY = oldObj.flipY,
                opacity = oldObj.opacity,
                shadow = oldObj.shadow,
                visible = oldObj.visible,
                backgroundColor = oldObj.backgroundColor,
                fillRule = oldObj.fillRule,
                paintFirst = oldObj.paintFirst,
                globalCompositeOperation = oldObj.globalCompositeOperation,
                skewX = oldObj.skewX,
                skewY = oldObj.skewY,
                cropX = oldObj.cropX,
                cropY = oldObj.cropY,
                src = oldObj.src,
                crossOrigin = oldObj.crossOrigin,
                filters = oldObj.filters,
            };
            return newObj;
        }

        /// <summary>
        /// Maps properties from an old BaseObject to a new one.
        /// </summary>
        /// <param name="oldObj">The original BaseObject.</param>
        /// <param name="newRootId">The ID for the new BaseObject.</param>
        /// <returns>A new BaseObject with properties copied from the old one.</returns>
        private BaseObject MapToNewBaseObject(BaseObject oldObj, Guid newRootId)
        {
            var newBaseObject = new BaseObject
            {
                rootId = newRootId,
                type = oldObj.type,
                version = oldObj.version,
                originX = oldObj.originX,
                originY = oldObj.originY,
                left = oldObj.left,
                top = oldObj.top,
                width = oldObj.width,
                height = oldObj.height,
                fill = oldObj.fill,
                stroke = oldObj.stroke,
                strokeWidth = oldObj.strokeWidth,
                strokeDashArray = oldObj.strokeDashArray,
                strokeLineCap = oldObj.strokeLineCap,
                strokeDashOffset = oldObj.strokeDashOffset,
                strokeLineJoin = oldObj.strokeLineJoin,
                strokeUniform = oldObj.strokeUniform,
                strokeMiterLimit = oldObj.strokeMiterLimit,
                scaleX = oldObj.scaleX,
                scaleY = oldObj.scaleY,
                angle = oldObj.angle,
                flipX = oldObj.flipX,
                flipY = oldObj.flipY,
                opacity = oldObj.opacity,
                shadow = oldObj.shadow,
                visible = oldObj.visible,
                backgroundColor = oldObj.backgroundColor,
                fillRule = oldObj.fillRule,
                paintFirst = oldObj.paintFirst,
                globalCompositeOperation = oldObj.globalCompositeOperation,
                skewX = oldObj.skewX,
                skewY = oldObj.skewY,
                cropX = oldObj.cropX,
                cropY = oldObj.cropY,
                src = oldObj.src,
                crossOrigin = oldObj.crossOrigin
            };
            return newBaseObject;
        }
    }
}
