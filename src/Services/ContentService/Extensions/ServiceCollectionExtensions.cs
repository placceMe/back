using ContentService.Data;
using ContentService.Repositories;
using ContentService.Services;
using ContentService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // Repository registrations
        services.AddScoped<IFaqRepository, FaqRepository>();
        services.AddScoped<IMainBannerRepository, MainBannerRepository>();

        // Service registrations
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<IMainBannerService, MainBannerService>();

        return services;
    }
}