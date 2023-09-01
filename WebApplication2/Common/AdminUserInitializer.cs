using AdvertisingAgency.Data.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace AdvertisingAgency.Web.Common
{
    /// <summary>
    /// Provides methods for initializing the admin user and its role.
    /// </summary>
    public class AdminUserInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AdminUserInitializer class.
        /// </summary>
        /// <param name="serviceProvider">The IServiceProvider instance.</param>
        /// <param name="configuration">The IConfiguration instance.</param>
        public AdminUserInitializer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _configuration = configuration;
        }

        /// <summary>
        /// Creates the admin user and assigns the "Admin" role.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateAdminUser()
        {
            var adminEmail = _configuration["Admin:Email"];
            var adminPassword = _configuration["Admin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // Няма конфигурация за администраторския потребител. Излизане от метода.
                return;
            }

            var userExists = await _userManager.FindByNameAsync(adminEmail);
            if (userExists == null)
            {
                // Няма потребител, който съответства на администраторския имейл.
                // Пропускаме останалата логика и излизаме от метода.
                return;
            }

            var isInRole = await _userManager.IsInRoleAsync(userExists, "Admin");
            if (!isInRole)
            {
                await _userManager.AddToRoleAsync(userExists, "Admin");
                if (await _userManager.IsInRoleAsync(userExists, "Customer"))
                {
                    await _userManager.RemoveFromRoleAsync(userExists, "Customer");
                }
            }
        }
    }
}

