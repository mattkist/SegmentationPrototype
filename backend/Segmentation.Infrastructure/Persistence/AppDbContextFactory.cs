using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Segmentation.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core tools (dotnet ef migrations) at design time.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=segmentation.design.db")
            .Options;

        return new AppDbContext(options);
    }
}
