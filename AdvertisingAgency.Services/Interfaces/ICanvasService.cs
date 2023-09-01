using AdvertisingAgency.Web.ViewModels.DTOs;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface ICanvasService
    {
        Task<IEnumerable<AdvertisingAgency.Data.Data.Models.Canvas>> GetCanvasesAsync();
        Task<CanvasDto> GetCanvasAsync(Guid id);
        Task<AdvertisingAgency.Data.Data.Models.Canvas> CreateCanvasAsync(AdvertisingAgency.Data.Data.Models.Canvas canvas);
        Task UpdateCanvasAsync(Guid id, AdvertisingAgency.Data.Data.Models.Canvas canvas);
        Task DeleteCanvasAsync(Guid id);
        bool CanvasExists(Guid id);
    }
}
