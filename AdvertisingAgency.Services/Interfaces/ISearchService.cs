using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Web.ViewModels.ViewModels;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface ISearchService
    {
        Task<List<ProjectSearchViewModel>> SearchProjectsAsync(string searchTerm);

        Task<List<ApplicationUser>> SearchUsersAsync(string searchTerm);
    }
}
