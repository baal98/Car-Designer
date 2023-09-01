using AdvertisingAgency.Services.Data.Models.ProjectSharing;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface IProjectSharingService
    {
        Task ShareProjectAsync(Guid projectId, string userId, string thumbnail);

        Task<string> AddToCollectionAsync(Guid canvasId, string userId);

        Task<List<SharedProjectViewModel>> GetSharedProjectsAsync(string userId);

        Task DeleteProjectAsync(Guid projectId);

        Task<SharedProjectViewModel> GetProjectDetails(Guid? id);
    }
}
