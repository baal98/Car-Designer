using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing canvas-related operations using MVC-based views.
    /// </summary>
    public class CanvasMvcController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICanvasMVCService _canvasService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly HtmlSanitizer _sanitizer;

        public CanvasMvcController(UserManager<ApplicationUser> userManager, ICanvasMVCService canvasService, IWebHostEnvironment hostingEnvironment, HtmlSanitizer sanitizer)
        {
            _userManager = userManager;
            _canvasService = canvasService;
            _hostingEnvironment = hostingEnvironment;
            _sanitizer = sanitizer;
        }

        /// <summary>
        /// Displays the single page application (SPA) view for canvas.
        /// </summary>
        /// <returns>The view containing canvas-related images.</returns>
        public IActionResult SPAView()
        {
            var decalsDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Decals");
            var decalsImages = Directory.EnumerateFiles(decalsDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            var brandsDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Brands");
            var brandsImages = Directory.EnumerateFiles(brandsDir)
                .Where(file => file.EndsWith(".png")
                                  || file.EndsWith(".jpg")
                                  || file.EndsWith(".svg")
                                  || file.EndsWith(".bmp")
                                  || file.EndsWith(".gif"))
                .Select(Path.GetFileName)
                .ToList();


            var offroadDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Offroad");
            var offroadImages = Directory.EnumerateFiles(offroadDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            var funnyDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Funny");
            var funnyImages = Directory.EnumerateFiles(funnyDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            ViewBag.DecalsImageNames = decalsImages.Count > 0 ? decalsImages : null;
            ViewBag.BrandsImageNames = brandsImages.Count > 0 ? brandsImages : null;
            ViewBag.OffroadImageNames = offroadImages.Count > 0 ? offroadImages : null;
            ViewBag.FunnyImageNames = funnyImages.Count > 0 ? funnyImages : null;

            return View();
        }

        /// <summary>
        /// Displays the view for free use of the SPA.
        /// </summary>
        /// <returns>The view containing freely usable canvas-related images.</returns>
        [AllowAnonymous]
        public IActionResult SPA_FreeUse()
        {
            var decalsDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Decals");
            var decalsImages = Directory.EnumerateFiles(decalsDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            var brandsDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Brands");
            var brandsImages = Directory.EnumerateFiles(brandsDir)
                .Where(file => file.EndsWith(".png")
                               || file.EndsWith(".jpg")
                               || file.EndsWith(".svg")
                               || file.EndsWith(".bmp")
                               || file.EndsWith(".gif"))
                .Select(Path.GetFileName)
                .ToList();


            var offroadDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Offroad");
            var offroadImages = Directory.EnumerateFiles(offroadDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            var funnyDir = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Gallery", "Funny");
            var funnyImages = Directory.EnumerateFiles(funnyDir, "*.svg")
                .Select(Path.GetFileName)
                .ToList();

            ViewBag.DecalsImageNames = decalsImages.Count > 0 ? decalsImages : null;
            ViewBag.BrandsImageNames = brandsImages.Count > 0 ? brandsImages : null;
            ViewBag.OffroadImageNames = offroadImages.Count > 0 ? offroadImages : null;
            ViewBag.FunnyImageNames = funnyImages.Count > 0 ? funnyImages : null;

            return View();
        }

        /// <summary>
        /// Displays the index view showing canvas projects.
        /// </summary>
        /// <param name="page">The page number of the projects to display.</param>
        /// <returns>The view containing canvas projects.</returns>
        public async Task<IActionResult> Index(int page = 1)
        {
            int projectsPerPage = 12;

            var user = await _userManager.GetUserAsync(User);

            var projects = await _canvasService.GetCanvasProjects(user, page, projectsPerPage);

            int totalPages = await _canvasService.GetTotalPages(user, projectsPerPage);

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(projects);
        }

        /// <summary>
        /// Displays the details view for a specific canvas.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas.</param>
        /// <returns>The view containing canvas details.</returns>
        [Route("/canvasmvc/details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = await _canvasService.GetCanvasDetails(id, userId);

            if (viewModel == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(viewModel);
        }

        /// <summary>
        /// Displays the create view for a new canvas.
        /// </summary>
        /// <returns>The view for creating a new canvas.</returns>
        public IActionResult Create()
        {
            ViewBag.UserId = _userManager.GetUserId(User);
            return View();
        }

        /// <summary>
        /// Handles the creation of a new canvas.
        /// </summary>
        /// <param name="canvas">The canvas data for creation.</param>
        /// <returns>The result of the create operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Canvas canvas)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _canvasService.CreateCanvas(user, canvas);
            if (!result)
            {
                return BadRequest("Invalid user ID format.");
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the edit view for a specific canvas.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to edit.</param>
        /// <returns>The view for editing the specified canvas.</returns>
        public async Task<IActionResult> Edit(Guid? id)
        {
            var userId = _userManager.GetUserId(User);
            var canvas = await _canvasService.GetCanvasForEdit(id, userId);

            if (canvas == null)
            {
                return NotFound();
            }

            return View(canvas);
        }

        /// <summary>
        /// Handles the editing of a specific canvas.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to edit.</param>
        /// <param name="canvas">The updated canvas data.</param>
        /// <returns>The result of the edit operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description")] Canvas canvas)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _canvasService.EditCanvas(id, userId, canvas);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the delete view for a specific canvas.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to delete.</param>
        /// <returns>The view for confirming the deletion of the specified canvas.</returns>
        public async Task<IActionResult> Delete(Guid? id)
        {
            var userId = _userManager.GetUserId(User);
            var canvas = await _canvasService.GetCanvasForDelete(id, userId);

            if (canvas == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(canvas);
        }

        /// <summary>
        /// Handles the confirmed deletion of a specific canvas.
        /// </summary>
        /// <param name="id">The unique identifier of the canvas to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _canvasService.DeleteCanvas(id, userId);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
