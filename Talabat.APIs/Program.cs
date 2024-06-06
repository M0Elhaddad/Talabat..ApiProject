using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using StackExchange.Redis;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Service.Contract;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.Identity;
using Talabat.Service;

namespace Talabat.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var WebApplicationBuilder = WebApplication.CreateBuilder(args);

            #region Configure Services
            // Add services to the Dependency Injection container.

            WebApplicationBuilder.Services.AddControllers(); // Register Required WebAPIs Services To The Dependency Injection Container

            WebApplicationBuilder.Services.AddSwaggerServices();

            // StoreContextDB
            WebApplicationBuilder.Services.AddDbContext<StoreContext>(options =>
            {
                options/*.UseLazyLoadingProxies()*/.UseSqlServer(WebApplicationBuilder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // IdentityDbContext
            WebApplicationBuilder.Services.AddDbContext<AppIdentityDbContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer(WebApplicationBuilder.Configuration.GetConnectionString("IdentityConnection"));
            });

            // RedisDB
            WebApplicationBuilder.Services.AddSingleton<IConnectionMultiplexer>((serviceProvider) =>
            {
                var connection = WebApplicationBuilder.Configuration.GetConnectionString("Redis");

                return ConnectionMultiplexer.Connect(connection);
            });

            // AddApplicationServices
            WebApplicationBuilder.Services.AddApplicationServices();

            // AddIdentityServices
            WebApplicationBuilder.Services.AddIdentityServices(WebApplicationBuilder.Configuration);

            // Cors Services
            WebApplicationBuilder.Services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy("MyPolicy",corsPolicyOptions =>
                {
                    corsPolicyOptions.AllowAnyHeader().AllowAnyMethod().WithOrigins(WebApplicationBuilder.Configuration["FrontBaseUrl"]);
                });
            });

            #endregion

            var app = WebApplicationBuilder.Build();

            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var _dbContext = services.GetRequiredService<StoreContext>(); // Ask CLR For Creating Object from DbContext Explicitly

            var _identityDbContext = services.GetRequiredService<AppIdentityDbContext>();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await _dbContext.Database.MigrateAsync(); // Update Database

                await StoreContextSeed.SeedAsync(_dbContext); // Data Seeding

                await _identityDbContext.Database.MigrateAsync();  // Update Database 

                var _userManger = services.GetRequiredService<UserManager<AppUser>>(); // Explicitly

                await AppIdentityDbContextSeed.SeedUsersAsync(_userManger);


            }
            catch (Exception ex)
            {

                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "an error has been occured during apply the migration");
            }


            #region Configure Kestrell MiddleWares
            // Configure the HTTP request pipeline.

            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerMiddleware();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("MyPolicy");

            app.MapControllers();

            app.UseAuthentication();

            app.UseAuthorization();
            #endregion

            app.Run();
        }
    }
}