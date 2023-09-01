using AdvertisingAgency.Data.Data.Models.Chat;
using AutoMapper;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Web.ViewModels.ViewModels;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    /// <summary>
    /// Represents a profile for AutoMapper configuration.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperProfile"/> class.
        /// Configures the mapping between different types.
        /// </summary>
        public AutoMapperProfile()
        {
            CreateMap<BaseObject, BaseObjectDto>();
            CreateMap<CanvasObject, CanvasObjectDto>();
            CreateMap<Canvas, CanvasDto>()
                .ForMember(dest => dest.BaseObject, opt => opt.MapFrom(src => src.BaseObject))
                .ForMember(dest => dest.CanvasObjects, opt => opt.MapFrom(src => src.Objects));
            CreateMap<ChatMessage, Message>()
                .ForMember(x => x.Username, opt => opt.MapFrom(x => x.User == null ? default : x.User.UserName))
                .ForMember(x => x.UserImageUrl, opt => opt.MapFrom(x => x.User == null ? default : x.User.ImageUrl))
                .ForMember(x => x.Text, opt => opt.MapFrom(src => src.Text))
                .ForMember(x => x.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(x => x.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<Country, CountryDto>();
            CreateMap<City, CityDto>();
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City));

            CreateMap<SharedProject, SharedProjectDTO>()
                .ForMember(dest => dest.Canvas, opt => opt.MapFrom(src => src.Canvas))
                .ForMember(dest => dest.SharingUser, opt => opt.MapFrom(src => src.SharingUser));

            CreateMap<ApplicationUser, ApplicationUserDTO>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ChatMessages, opt => opt.MapFrom(src => src.ChatMessages))
                .ForMember(dest => dest.Canvases, opt => opt.MapFrom(src => src.Canvases));

            CreateMap<ShoppingCart, shoppingCartVM>()
                .ForMember(dest => dest.ShoppingCartItems, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.ShoppingCartTotal,
                    opt => opt.MapFrom(src =>
                        src.Items.Sum(i =>
                            i.Quantity *
                            i.Product.Price)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));
        }
    }
}
