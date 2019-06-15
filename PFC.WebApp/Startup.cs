using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PFC.WebApp.Data;
using PFC.WebApp.Models;
using PFC.WebApp.Services;
using PFC.WebApp.Services.Providers;
using PFC.WebApp.Services.DocumentManager;
using PFC.WebApp.Services.DocumentManager.Providers.DocumentManagerDiskProvider;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace PFC.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            Configuration = config;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddTransient<IEmailSender>(x => new ConsoleEmailSender());
            }
            else
            {
                var host = Configuration.GetValue<string>("AppSettings:EmailSender.Host");
                var port = int.Parse(Configuration.GetValue<string>("AppSettings:EmailSender.Port"));
                var user = Configuration.GetValue<string>("AppSettings:EmailSender.User");
                var password = Configuration.GetValue<string>("AppSettings:EmailSender.Password");
                services.AddTransient<IEmailSender>(x => new EmailSender(host, port, user, password));
            }
            services.AddTransient<IContentTypeProvider, FileExtensionContentTypeProvider>();

            var documentMAnagerPath = Configuration.GetValue<string>("AppSettings:DocumentManagerDiskProviderRepositoryPath");
            services.AddTransient<IDocumentRepositoryManager>(x => new DocumentRepositoryManager(documentMAnagerPath));
            services.AddLogging(x => x.AddConsole());

            services.AddMvc()
                .AddJsonOptions(x => x.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver())
                .AddMvcOptions(options =>
                {
                    AuthorizationPolicy filterPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    AuthorizeFilter filter = new AuthorizeFilter(filterPolicy);
                    options.Filters.Add(filter);
                });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            InitializeDatabase(app).Wait();

        }

        private async Task InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices
                            .GetService<IServiceScopeFactory>().CreateScope())
            {
                // Ejecución de migraciones
                using (var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>())
                {
                    await context.Database.MigrateAsync();

                    // Creamos los roles
                    using (var roleManager = scope.ServiceProvider
                        .GetRequiredService<RoleManager<IdentityRole>>())
                    {
                        if (!roleManager.Roles.Any())
                        {
                            await roleManager.CreateAsync(new IdentityRole()
                            {
                                Name = "SuperAdmin",
                                NormalizedName = "Super Administrador"
                            });
                            await roleManager.CreateAsync(new IdentityRole()
                            {
                                Name = "Gestor",
                                NormalizedName = "Administración"
                            });
                            await roleManager.CreateAsync(new IdentityRole()
                            {
                                Name = "Comercial",
                                NormalizedName = "Comercial"
                            });
                        }
                    }

                    using (var userManager = scope.ServiceProvider
                        .GetRequiredService<UserManager<ApplicationUser>>())
                    {
                        // Creamos un usuario con derechos de supervaca
                        if (!userManager.Users.Any())
                        {
                            var adminUser = Configuration.GetValue<string>("AppSettings:Admin.User");
                            var adminEmail = Configuration.GetValue<string>("AppSettings:Admin.Email");
                            var adminPassword = Configuration.GetValue<string>("AppSettings:Admin.Password");
                            ApplicationUser superAdminUser = new ApplicationUser()
                            {
                                UserName = adminUser,
                                Email = adminEmail,
                                EmailConfirmed = true,
                            };

                            await userManager.CreateAsync(superAdminUser, adminPassword);
                            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                        }
                    }
                }
            }
        }
    }
}
