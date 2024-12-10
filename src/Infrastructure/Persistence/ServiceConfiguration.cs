using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;
using Shared.Services;

namespace Persistence

{

    public static class ServiceExtensions
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));





            //Agregamos una inyeccion donde le decimos que un tipo de RepositoryAsync va a implementar
            //un IRepositoryAsync cualquiera sea
            #region Repositories
            services
                .AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork))
                .AddTransient(typeof(IRepositoryAsync<>), typeof(RepositoryAsync<>))
                .AddTransient(typeof(ICurrentUserService), typeof(CurrentUserService));
            #endregion

            #region Caching
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    //options.Configuration = "149.50.128.254:6379";
            //    options.Configuration = configuration.GetValue<string>("Caching:RedisConnection");
            //});
            #endregion


        }
    }
}