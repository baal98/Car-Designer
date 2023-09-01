namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Country and City information
        public int? CountryId { get; set; }
        public CountryDto Country { get; set; }
        public int? CityId { get; set; }
        public CityDto City { get; set; }
    }
}
