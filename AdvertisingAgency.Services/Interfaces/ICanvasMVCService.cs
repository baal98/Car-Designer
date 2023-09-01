using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Data.Models.Canvas;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface ICanvasMVCService
    {
        Task<IQueryable<Canvas>> GetCanvasProjects(ApplicationUser user, int page, int projectsPerPage);
        Task<int> GetTotalPages(ApplicationUser user, int projectsPerPage);
        Task<CanvasViewModel> GetCanvasDetails(Guid? id, string userId);
        Task<bool> CreateCanvas(ApplicationUser user, AdvertisingAgency.Data.Data.Models.Canvas canvas);
        Task<AdvertisingAgency.Data.Data.Models.Canvas> GetCanvasForEdit(Guid? id, string userId);
        Task<bool> EditCanvas(Guid id, string userId, AdvertisingAgency.Data.Data.Models.Canvas canvas);
        Task<AdvertisingAgency.Data.Data.Models.Canvas> GetCanvasForDelete(Guid? id, string userId);
        Task<bool> DeleteCanvas(Guid id, string userId);
        Task<bool> CanvasExists(Guid id);
    }
}