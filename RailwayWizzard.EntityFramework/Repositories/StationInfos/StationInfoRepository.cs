using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos
{
    /// <inheritdoc/>
    public class StationInfoRepository : IStationInfoRepository
    {
        private readonly RailwayWizzardAppContext _context;

        public StationInfoRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<StationInfo?> FindByStationNameAsync(string stationName)
        {
            return await _context.StationInfo
                .SingleOrDefaultAsync(s => s.StationName == stationName);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfo>> ContainsByStationNameAsync(string stationName)
        {
            return await _context.StationInfo
                .Where(s => s.StationName.Contains(stationName))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> AnyByExpressCodeAsync(long expressCode)
        {
            return await _context.StationInfo
                .AnyAsync(s => s.ExpressCode == expressCode);
        }

        /// <inheritdoc/>
        public async Task AddRangeStationInfoAsync(IReadOnlyCollection<StationInfo> stationInfo)
        {
            await _context.StationInfo.AddRangeAsync(stationInfo);

            await _context.SaveChangesAsync();
        }
    }
}