using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class CropSeasonReadService(AppDbContext db) : ICropSeasonReadService
{
    public async Task<IReadOnlyList<CropSeasonDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await db.CropSeasons.AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CropSeasonDto(c.Id, c.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.CropSeasons.AsNoTracking().AnyAsync(c => c.Id == cropSeasonId, cancellationToken);
}
