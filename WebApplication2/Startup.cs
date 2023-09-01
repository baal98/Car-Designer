using AdvertisingAgency.Data.Common;
using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.AzureStorage;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.Common;
using AdvertisingAgency.Web.Hubs;
using AdvertisingAgency.Web.ViewModels.DTOs;
using Azure.Core.Extensions;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Ganss.Xss;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.FileProviders;

namespace AdvertisingAgency.Web
{
    /// <summary>
    /// Configures the application services and HTTP pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration instance.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //use for deploying in Azure
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddRoles<IdentityRole>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });

            services.AddRazorPages();
            services.AddTransient<IChatService, ChatService>();
            services.AddSignalR();
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });
            services.AddControllersWithViews();
            services.AddScoped<IImageService, ImageService>();
            services.AddTransient<ISmsService, SmsService>();
            services.AddTransient<ICanvasService, CanvasService>();
            services.AddTransient<ICanvasMVCService, CanvasMVCService>();
            services.AddTransient<IProjectSharingService, ProjectSharingService>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddSingleton<HtmlSanitizer>();
            services.AddResponseCompression();
            services.AddMemoryCache();
            services.AddResponseCaching();


            // Променяме тук, за да получим новата конфигурация за AzureStorageConfig
            var azureStorageConfig = Configuration.GetSection("AzureStorageConfig").Get<AzureStorageConfig>();
            //services.AddSingleton(x => new AzureStorageService(azureStorageConfig, "cardesigner"));
            services.AddSingleton<IAzureStorageService, AzureStorageService>(provider =>
                new AzureStorageService(azureStorageConfig, "cardesigner"));

        }

        /// <summary>
        /// Configures the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Конфигуриране на HTTP пайплайна и настройки:
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            // Настройване на статичните файлове
            app.UseStaticFiles(); // За да може да се достъпват файлове в wwwroot
            app.UseStaticFiles(new StaticFileOptions // За SPA приложението
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Original_WorkVersion")),
                RequestPath = "/Original_WorkVersion"
            });

            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = "/Home/Error/404";
                    await next();
                }
                else if (context.Response.StatusCode == 500 && !context.Response.HasStarted)
                {
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = "/Home/Error/500";
                    await next();
                }
            });


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "CanvasMvc",
                    pattern: "CanvasMvc/{action=Index}/{id?}",
                    defaults: new { controller = "CanvasMvc" });

                endpoints.MapControllerRoute(
                    name: "Project",
                    pattern: "Project/{action=Index}/{id?}",
                    defaults: new { controller = "Project" });


                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapFallbackToFile("/Index");

                endpoints.MapRazorPages();

                endpoints.MapHub<ChatHub>("/chat");
            });

            RoleInitializer.InitializeAsync(userManager, roleManager).Wait();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var initializer = new AdminUserInitializer(serviceScope.ServiceProvider, Configuration);
                initializer.CreateAdminUser().Wait();
            }

        }
    }
}