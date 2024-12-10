using Application.Common.Interfaces;
using Application.Common.Wrappers;
using Domain.Settings;
using Identity.Context;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

namespace Identity
{
    public static class ServiceExtensions
    {
        public static void AddIdentityInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("IdentityConnection"),
                    b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

            services.AddTransient<IAccountService, AccountService>();

            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"])),
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


        }
    }
}
