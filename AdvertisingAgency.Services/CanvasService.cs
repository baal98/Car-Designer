using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    public class CanvasService : ICanvasService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CanvasService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all Canvas items, including their Objects and BaseObject.
        /// </summary>
        public async Task<IEnumerable<Canvas>> GetCanvasesAsync()
        {
            return await _context.Canvases
                .Include(c => c.Objects)
                .Include(o => o.BaseObject)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single Canvas item by its ID and maps it to a DTO.
        /// </summary>
        /// <param name="id">The ID of the Canvas to retrieve.</param>
        public async Task<CanvasDto> GetCanvasAsync(Guid id)
        {
            var canvas = await _context.Canvases
                .Include(c => c.BaseObject)
                .Include(c => c.Objects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (canvas == null)
            {
                return null;
            }

            return _mapper.Map<CanvasDto>(canvas);
        }

        /// <summary>
        /// Creates a new Canvas entry in the database.
        /// </summary>
        /// <param name="canvas">The Canvas entity to be created.</param>
        public async Task<Canvas> CreateCanvasAsync(Canvas canvas)
        {
            if (canvas.BaseObject != null)
            {
                _context.BaseObjects.Add(canvas.BaseObject);
            }

            if (canvas.Objects != null)
            {
                foreach (var obj in canvas.Objects)
                {
                    _context.objects.Add(obj);
                }
            }

            _context.Canvases.Add(canvas);

            await _context.SaveChangesAsync();

            return canvas;
        }

        /// <summary>
        /// Updates an existing Canvas entry in the database.
        /// </summary>
        /// <param name="id">The ID of the Canvas to update.</param>
        /// <param name="canvas">The updated Canvas entity.</param>
        public async Task UpdateCanvasAsync(Guid id, Canvas canvas)
        {
            var existingCanvas = _context.Canvases
                .Include(c => c.BaseObject)
                .Include(c => c.Objects)
                .FirstOrDefault(c => c.Id == id);

            if (existingCanvas == null)
            {
                throw new Exception("Canvas not found");
            }

            existingCanvas.Name = canvas.Name;
            existingCanvas.Description = canvas.Description;

            if (canvas.BaseObject != null)
            {
                existingCanvas.BaseObject = canvas.BaseObject;
            }
            else if (existingCanvas.BaseObject != null)
            {
                _context.Remove(existingCanvas.BaseObject);
                existingCanvas.BaseObject = null;
            }

            if (canvas.Objects != null)
            {
                foreach (var newObj in canvas.Objects)
                {
                    var existingObj = existingCanvas.Objects.FirstOrDefault(o => o.Id == newObj.Id);

                    if (existingObj != null)
                    {
                        _context.Entry(existingObj).CurrentValues.SetValues(newObj);
                    }
                }

                var newObjects = canvas.Objects.Where(newObj =>
                    newObj.Id == 0 || !existingCanvas.Objects.Any(existingObj => existingObj.Id == newObj.Id)).ToList();

                foreach (var newObj in newObjects)
                {
                    existingCanvas.Objects.Add(newObj);
                }

                var objectsToRemove = existingCanvas.Objects.Where(o => !canvas.Objects.Any(newObj => newObj.Id == o.Id))
                    .ToList();
                foreach (var objToRemove in objectsToRemove)
                {
                    existingCanvas.Objects.Remove(objToRemove);
                }
            }

            if (canvas.Thumbnail != null)
            {
                existingCanvas.Thumbnail = canvas.Thumbnail;
            }

            _context.Entry(existingCanvas).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CanvasExists(id))
                {
                    throw new Exception("Canvas not found");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes a Canvas entry from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the Canvas to delete.</param>
        public async Task DeleteCanvasAsync(Guid id)
        {
            var canvas = await _context.Canvases.FindAsync(id);
            if (canvas == null)
            {
                throw new Exception("Canvas not found");
            }

            _context.Canvases.Remove(canvas);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a Canvas entry exists in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the Canvas to check.</param>
        public bool CanvasExists(Guid id)
        {
            return _context.Canvases.Any(c => c.Id == id);
        }
    }
}
