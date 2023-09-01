using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Common;
using AdvertisingAgency.Services.Data.Models.ProjectSharing;
using AdvertisingAgency.Services.ProjectSharing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AdvertisingAgency.Services.Interfaces;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing project-sharing related operations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : BaseController
    {
        private readonly IProjectSharingService _projectSharingService;

        /// <summary>
        /// Initializes a new instance of the ProjectController class with the specified project sharing service.
        /// </summary>
        /// <param name="projectSharingService">The service responsible for project-sharing related operations.</param>
        public ProjectController(IProjectSharingService projectSharingService)
        {
            _projectSharingService = projectSharingService;
        }

        /// <summary>
        /// Shares a project with other users.
        /// </summary>
        /// <param name="model">The view model containing project sharing information.</param>
        /// <returns>The result of the project sharing operation.</returns>
        [HttpPost("share")]
        public async Task<IActionResult> ShareProject([FromForm] SharedProjectViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _projectSharingService.ShareProjectAsync(model.CanvasId, userId, model.Thumbnail);
                TempData["SuccessMessage"] = "Project successfully shared!";
                return RedirectToAction(nameof(SharedProjects));
            }
            catch (ProjectSharingService.CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(SharedProjects));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unknown error occurred. Please try again.";
                return RedirectToAction(nameof(SharedProjects));
            }
        }

        /// <summary>
        /// Adds a project to the user's collection.
        /// </summary>
        /// <param name="model">The view model containing project information to add to the collection.</param>
        /// <returns>The result of adding the project to the collection.</returns>
        [HttpPost("add-to-my-collection")]
        public async Task<IActionResult> AddToMyCollection([FromForm] AddToCollectionViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var success = await _projectSharingService.AddToCollectionAsync(model.CanvasId, userId);
                TempData["SuccessMessage"] = "Project successfully added to your collection!";
                return RedirectToAction("Index", "CanvasMvc");
                
            }
            catch (ProjectSharingService.CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(SharedProjects));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unknown error occurred. Please try again.";
                return RedirectToAction(nameof(SharedProjects));
            }
        }

        /// <summary>
        /// Displays shared projects paginated in the view.
        /// </summary>
        /// <param name="page">The current page number for pagination.</param>
        /// <param name="pageSize">The number of projects per page for pagination.</param>
        /// <returns>The view displaying shared projects.</returns>
        [HttpGet("shared-projects")]
        public async Task<IActionResult> SharedProjects(int page = 1, int pageSize = 12)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sharedProjects = await _projectSharingService.GetSharedProjectsAsync(userId);

                var paginatedProjects = sharedProjects.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                var model = new PaginatedList<SharedProjectViewModel>(paginatedProjects, sharedProjects.Count, page, pageSize);

                return View(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a shared project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <returns>The result of the project deletion operation.</returns>
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteProject([FromForm] Guid projectId)
        {
            try
            {
                await _projectSharingService.DeleteProjectAsync(projectId);
                TempData["SuccessMessage"] = "Project successfully deleted!";
                return RedirectToAction(nameof(SharedProjects));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(SharedProjects));
            }
        }

        /// <summary>
        /// Displays details of a shared project.
        /// </summary>
        /// <param name="id">The unique identifier of the project to display details for.</param>
        /// <returns>The view displaying details of the shared project.</returns>
        [HttpGet("shared-projects-details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            var viewModel = await _projectSharingService.GetProjectDetails(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }
    }
}
