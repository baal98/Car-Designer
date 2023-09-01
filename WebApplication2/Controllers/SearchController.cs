using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for performing search-related operations.
    /// </summary>
    public class SearchController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISearchService _searchService;

        /// <summary>
        /// Initializes a new instance of the SearchController class with the specified user manager and search service.
        /// </summary>
        /// <param name="userManager">The user manager for managing user-related operations.</param>
        /// <param name="searchService">The service responsible for search-related operations.</param>
        public SearchController(UserManager<ApplicationUser> userManager, ISearchService searchService)
        {
            _userManager = userManager;
            _searchService = searchService;
        }

        /// <summary>
        /// Displays search results for projects.
        /// </summary>
        /// <param name="searchTerm">The search term to look for in projects.</param>
        /// <returns>The view displaying search results for projects.</returns>
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var projects = await _searchService.SearchProjectsAsync(searchTerm);
                return View(projects);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Displays search results for users, filtered for administrators.
        /// </summary>
        /// <param name="searchTerm">The search term to look for in users.</param>
        /// <returns>The view displaying search results for users.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> UserSearch(string searchTerm)
        {
            try
            {
                var users = await _searchService.SearchUsersAsync(searchTerm);
                var currentUserId = _userManager.GetUserId(User);
                var filteredUsers = users.Where(u => u.Id != currentUserId).ToList();
                return View("~/Views/Users/SearchedUsers.cshtml", filteredUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
