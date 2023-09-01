using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.DTOs;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    /// <summary>
    /// Provides methods to perform search operations on projects and users.
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="mapper">The AutoMapper instance for mapping objects.</param>
        public SearchService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Searches for projects based on a given term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A list of projects matching the search term.</returns>
        public async Task<List<ProjectSearchViewModel>> SearchProjectsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty");
            }

            var projects = await _context.SharedProjects
                .Include(sp => sp.Canvas)
                .Include(sp => sp.SharingUser)
                .Include(sp => sp.Canvas.BaseObject)
                .Include(sp => sp.Canvas.Objects)
                .Where(p => p.Canvas.Name.Contains(searchTerm) || (p.Canvas.Description != null && p.Canvas.Description.Contains(searchTerm)))
                .ToListAsync();

            var searchedCanvases = new List<ProjectSearchViewModel>();

            foreach (var project in projects)
            {
                var userName = await _context.ApplicationUsers
                    .Where(u => u.Id == project.SharingUserId.ToString())
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                var projectViewModel = new ProjectSearchViewModel
                {
                    Id = project.CanvasId.GetValueOrDefault(),
                    UserId = project.SharingUserId.GetValueOrDefault(),
                    Canvas = project.Canvas,
                    Name = project.Canvas?.Name,
                    Description = project.Canvas?.Description,
                    Thumbnail = project.Canvas?.Thumbnail,
                    Username = userName,
                    CreatedOn = project.Canvas.CreatedOn.Value,
                };

                searchedCanvases.Add(projectViewModel);
            }
            return searchedCanvases;
        }

        /// <summary>
        /// Searches for users based on a given term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A list of users matching the search term.</returns>
        public async Task<List<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty");
            }

            var users = await _context.ApplicationUsers
                .Include(sp => sp.Addresses)
                .Include(sp => sp.City)
                .Include(sp => sp.Country)
                .Include(sp => sp.ShoppingCarts)
                .Include(sp => sp.OrderHeaders)
                .Where(p => p.Email.Contains(searchTerm) || (p.PhoneNumber.Contains(searchTerm)))
                .ToListAsync();

            var searchedUsers = _mapper.Map<List<ApplicationUserDTO>>(users);

            return users;
        }

    }
}
