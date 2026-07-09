using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Behaviors;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Services;
using System.Reflection;

namespace RealEstateApp.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            #endregion

            #region Services IOC
            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<IPropertyTypeService, PropertyTypeService>();
            services.AddScoped<ISaleTypeService, SaleTypeService>();
            services.AddScoped<IImprovementService, ImprovementService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();
            services.AddScoped<IFavoritePropertyService, FavoritePropertyService>();
            #endregion
        }
    }
}