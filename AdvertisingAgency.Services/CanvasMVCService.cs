using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Data.Models.Canvas;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    public class CanvasMVCService : ICanvasMVCService
    {
        private readonly ApplicationDbContext _context;

        public CanvasMVCService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a paged list of Canvas projects for a given user.
        /// </summary>
        public async Task<IQueryable<Canvas>> GetCanvasProjects(ApplicationUser user, int page, int projectsPerPage)
        {
            var userCanvases = _context.Canvases
                .Where(c => c.UserId.ToString() == user.Id)
                .Include(c => c.BaseObject)
                .Include(o => o.Objects)
                .OrderBy(c => c.CreatedOn);

            var sharedProjects = _context.SharedProjects
                .Where(sp => sp.SharingUserId == Guid.Parse(user.Id))
                .OrderByDescending(sh => sh.CreatedOn)
                .Select(sp => sp.CanvasId);

            var projects = userCanvases
                .Where(c => !sharedProjects.Contains(c.Id))
                .OrderByDescending(c => c.CreatedOn)
                .Skip((page - 1) * projectsPerPage)
                .Take(projectsPerPage);

            return projects;
        }

        /// <summary>
        /// Calculates the total number of pages for the user's Canvas projects.
        /// </summary>
        public async Task<int> GetTotalPages(ApplicationUser user, int projectsPerPage)
        {
            var userCanvases = _context.Canvases
                .Where(c => c.UserId.ToString() == user.Id);

            int totalProjects = await userCanvases.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalProjects / projectsPerPage));

            return totalPages;
        }

        /// <summary>
        /// Gets detailed information for a specific Canvas project.
        /// </summary>
        public async Task<CanvasViewModel> GetCanvasDetails(Guid? id, string userId)
        {
            var canvas = await _context.Canvases
                .Include(c => c.BaseObject)
                .Include(o => o.Objects)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId.ToString() == userId);

            var categories = await _context.Categories.ToListAsync();

            if (canvas != null)
            {
                var viewModel = new CanvasViewModel
                {
                    Id = canvas.Id,
                    Canvas = canvas,
                    TotalPrice = canvas.Objects.Sum(o => o.price ?? 0),
                    Thumbnail = canvas.Thumbnail,
                    Description = canvas.Description,
                    Categories = categories.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = Enum.GetName(typeof(CategoryType), c.CategoryTypeName),
                        DisplayOrder = c.DisplayOrder,
                        CreatedDateTime = c.CreatedDateTime
                    }).ToList()
                };

                return viewModel;
            }

            return null;
        }

        /// <summary>
        /// Creates a new Canvas project for a user.
        /// </summary>
        public async Task<bool> CreateCanvas(ApplicationUser user, Canvas canvas)
        {
            if (Guid.TryParse(user.Id, out Guid userIdGuid))
            {
                canvas.UserId = userIdGuid;
                
                // Encode the Name and Description to prevent script injection
                canvas.Name = System.Net.WebUtility.HtmlEncode(canvas.Name);
                canvas.Description = System.Net.WebUtility.HtmlEncode(canvas.Description);

                _context.Add(canvas);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves a Canvas project for editing by a specific user.
        /// </summary>
        public async Task<Canvas> GetCanvasForEdit(Guid? id, string userId)
        {
            var canvas = await _context.Canvases.FindAsync(id);

            if (canvas == null || userId != canvas.UserId.ToString())
            {
                return null;
            }

            return canvas;
        }

        /// <summary>
        /// Edits an existing Canvas project.
        /// </summary>
        public async Task<bool> EditCanvas(Guid id, string userId, Canvas canvas)
        {
            if (id != canvas.Id)
            {
                return false;
            }

            var existingCanvas = await _context.Canvases.FindAsync(id);

            if (existingCanvas == null || userId != existingCanvas.UserId.ToString())
            {
                return false;
            }

            // Encode the Name and Description to prevent script injection
            canvas.Name = System.Net.WebUtility.HtmlEncode(canvas.Name);
            canvas.Description = System.Net.WebUtility.HtmlEncode(canvas.Description);

            existingCanvas.Name = canvas.Name;

            try
            {
                _context.Update(existingCanvas);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CanvasExists(canvas.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves a Canvas project for deletion by a specific user.
        /// </summary>
        public async Task<Canvas> GetCanvasForDelete(Guid? id, string userId)
        {
            var canvas = await _context.Canvases
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId.ToString() == userId);

            return canvas;
        }

        /// <summary>
        /// Deletes a Canvas project by a specific user.
        /// </summary>
        public async Task<bool> DeleteCanvas(Guid id, string userId)
        {
            var canvas = await _context.Canvases
                .Include(c => c.Objects)
                .Include(c => c.BaseObject)
                .SingleOrDefaultAsync(m => m.Id == id && m.UserId.ToString() == userId);

            if (canvas == null)
            {
                return false;
            }

            if (canvas.Objects != null)
            {
                _context.objects.RemoveRange(canvas.Objects);
            }

            if (canvas.BaseObject != null)
            {
                _context.BaseObjects.Remove(canvas.BaseObject);
            }

            await _context.SaveChangesAsync();

            _context.Canvases.Remove(canvas);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Checks if a Canvas project exists by its ID.
        /// </summary>
        public async Task<bool> CanvasExists(Guid id)
        {
            return await _context.Canvases.AnyAsync(c => c.Id == id);
        }
    }
}
