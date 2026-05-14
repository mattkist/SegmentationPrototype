using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Segmentation.Application.Abstractions;
using Segmentation.Infrastructure.Persistence;
using Segmentation.Infrastructure.Services;

namespace Segmentation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICropSeasonReadService, CropSeasonReadService>();
        services.AddScoped<IFarmerReadService, FarmerReadService>();
        services.AddScoped<IKpiReadService, KpiReadService>();
        services.AddScoped<IKpiImportService, KpiImportService>();
        services.AddScoped<ISegmentationConfigurationService, SegmentationConfigurationService>();
        services.AddScoped<ISegmentationSimulationService, SegmentationSimulationService>();

        return services;
    }
}
