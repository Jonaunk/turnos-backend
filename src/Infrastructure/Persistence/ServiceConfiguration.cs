using Application.Common.Interfaces;
using Application.Common.Wrappers;
using Domain.Entities.Users;
using Domain.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence.Contexts;
using Persistence.Repositories;
using Persistence.Services;
using System.Text;

namespace Persistence

{

    public static class ServiceExtensions
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


            //    // Register services here
            //services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

            //services.AddSingleton<MongoDbContext>(serviceProvider =>
            //{
            //    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            //    return new MongoDbContext(settings.ConnectionString, settings.DatabaseName);
            //});

            #region Services

            services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services
                .AddTransient<IAccountService, AccountService>();


            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]!)),
                };
                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        return c.Response.WriteAsync(c.Exception.ToString());
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new Response<string>("Usted no esta autorizado"));
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new Response<string>("Usted no tiene permisos sobre este recurso"));
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            #endregion

            //Agregamos una inyeccion donde le decimos que un tipo de RepositoryAsync va a implementar
            //un IRepositoryAsync cualquiera sea
            #region Repositories
            services
                .AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork))
                .AddTransient(typeof(IRepositoryAsync<>), typeof(RepositoryAsync<>));
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