using AdvertisingAgency.Data.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace AdvertisingAgency.Web.Common
{
    /// <summary>
    /// Provides methods for initializing roles in the application.
    /// </summary>
    public static class RoleInitializer
    {
        /// <summary>
        /// Initializes the predefined roles in the application.
        /// </summary>
        /// <param name="userManager">The UserManager instance.</param>
        /// <param name="roleManager">The RoleManager instance.</param>
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
