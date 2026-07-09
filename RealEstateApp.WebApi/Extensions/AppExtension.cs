using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning.ApiExplorer;

namespace RealEstateApp.WebApi.Extensions
{
    public static class AppExtension
    {
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions();
                if (versionDescriptions != null && versionDescriptions.Count > 0)
                {
                    foreach (var apiVersion in versionDescriptions)
                    {
                        var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                        var name = $"RealEstate API - {apiVersion.GroupName.ToUpperInvariant()}";
                        opt.SwaggerEndpoint(url, name);
                    }
                }
            });
        }
    }
}