using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Common;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Infrastructure.Exceptions;

namespace RailwayWizzard.Infrastructure.Repositories.StationsInfo
{
    /// <inheritdoc/>
    public class StationInfoRepository : IStationInfoRepository
    {
        private const int MaxStationsSearchCount = 20;
        
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StationInfoRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public StationInfoRepository(RailwayWizzardAppContext context)
        {
            _context = Ensure.NotNull(context);
        }

        /// <inheritdoc/>
        public async Task<StationInfoExtended> GetByIdAsync(int id)
        {
            var station = await _context.StationsInfoExtended.FirstOrDefaultAsync(x => x.Id == id);
            
            if(station == null)
                throw new EntityNotFoundException($"{typeof(StationInfoExtended)} with Id: {id} not found");

            return station;
        }
        
        /// <inheritdoc/>
        public async Task<StationInfoExtended> GetByNameAsync(string name)
        {
            var station = await FindByNameExactAsync(name);
            
            if(station == null)
                throw new EntityNotFoundException($"{typeof(StationInfoExtended)} with Name: {name} not found");

            return station;
        }

        /// <inheritdoc/>
        public async Task<StationInfoExtended?> FindByNameExactAsync(string name)
        {
            return await _context.StationsInfoExtended
                .SingleOrDefaultAsync(s => s.Name == name);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfoExtended>> FindByNameContainsAsync(string name)
        {
            var searchTerm = name.ToUpper();

            var stations = await _context.StationsInfoExtended
                .Where(s => s.Name.Contains(searchTerm))
                .ToListAsync();

            var result = stations
                .OrderByDescending(s => s.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ThenBy(s => s.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ThenBy(s => s.Name.Length)
                .Take(MaxStationsSearchCount)
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public async Task AddRangeStationInfosAsync(IReadOnlyCollection<StationInfoExtended> stationInfos)
        {
            foreach (var stationInfo in stationInfos)
            {
                var anyStationInfo = await AnyByExpressCodeAsync(stationInfo.ExpressCode);
            
                if (anyStationInfo is false)
                    _context.StationsInfoExtended.Add(stationInfo);                
            }
            
            await _context.SaveChangesAsync();
        }

        private async Task<bool> AnyByExpressCodeAsync(long expressCode)
        {
            return await _context.StationsInfoExtended
                .AnyAsync(s => s.ExpressCode == expressCode);
        }
    }
}