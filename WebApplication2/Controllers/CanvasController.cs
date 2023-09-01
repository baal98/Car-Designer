using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing canvas-related operations through API endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CanvasController : BaseController
    {
        private readonly ICanvasService _canvasService;

        /// <summary>
        /// Initializes a new instance of the CanvasController class with the specified canvas service.
        /// </summary>
        /// <param name="canvasService">The service responsible for canvas-related operations.</param>
        public CanvasController(ICanvasService canvasService)
        {
            _canvasService = canvasService;
        }

        /// <summary>
        /// Retrieves a list of canvases.
        /// </summary>
        /// <returns>An ActionResult containing the list of canvases.</returns>
        // GET: api/Canvas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Canvas>>> GetCanvases()
        {
            var canvases = await _canvasService.GetCanvasesAsync();
            return Ok(canvases);
        }

        /// <summary>
        /// Retrieves a specific canvas by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas.</param>
        /// <returns>An ActionResult containing the retrieved canvas DTO.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CanvasDto>> GetCanvas(Guid id)
        {
            var canvas = await _canvasService.GetCanvasAsync(id);

            if (canvas == null)
            {
                return NotFound();
            }

            return Ok(canvas);
        }

        /// <summary>
        /// Updates an existing canvas by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to be updated.</param>
        /// <param name="canvas">The updated canvas data.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        // PUT: api/Canvas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCanvas(Guid id, [FromBody] Canvas canvas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _canvasService.UpdateCanvasAsync(id, canvas);
            return NoContent();
        }

        /// <summary>
        /// Creates a new canvas.
        /// </summary>
        /// <param name="canvas">The canvas data for creation.</param>
        /// <returns>An ActionResult containing the created canvas.</returns>
        // POST: api/Canvas
        [HttpPost]
        public async Task<ActionResult<Canvas>> CreateCanvas(Canvas canvas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCanvas = await _canvasService.CreateCanvasAsync(canvas);
            return CreatedAtAction("GetCanvas", new { id = createdCanvas.Id }, createdCanvas);
        }

        /// <summary>
        /// Deletes a canvas by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to be deleted.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        // DELETE: api/Canvas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCanvas(Guid id)
        {
            await _canvasService.DeleteCanvasAsync(id);
            return NoContent();
        }
    }
}
