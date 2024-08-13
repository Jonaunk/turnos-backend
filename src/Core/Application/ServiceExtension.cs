using Application.Common.Behaviurs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;



namespace Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration _config)
        {

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            //Declaramos la instancia de Behaviours para que pase por aca el flujo y pueda usar las validaciones
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        }
    }
}