﻿using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Infrastructure.Exceptions;

namespace RailwayWizzard.Infrastructure.Repositories.StationsInfo
{
    /// <inheritdoc/>
    public class StationInfoRepository : IStationInfoRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StationInfoRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public StationInfoRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task<StationInfo> GetByIdAsync(int id)
        {
            var station = await _context.StationsInfo.FirstOrDefaultAsync(x => x.Id == id);
            
            if(station == null)
                throw new EntityNotFoundException($"{typeof(StationInfo)} with Id: {id} not found");

            return station;
        }
        
        public async Task<StationInfo> GetByNameAsync(string name)
        {
            var station = await FindByNameAsync(name);
            
            if(station == null)
                throw new EntityNotFoundException($"{typeof(StationInfo)} with Name: {name} not found");

            return station;
        }

        /// <inheritdoc/>
        public async Task<StationInfo?> FindByNameAsync(string name)
        {
            return await _context.StationsInfo
                .SingleOrDefaultAsync(s => s.Name == name);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfo>> ContainsByStationNameAsync(string name)
        {
            return await _context.StationsInfo
                .Where(s => s.Name.Contains(name))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> AnyByExpressCodeAsync(long expressCode)
        {
            return await _context.StationsInfo
                .AnyAsync(s => s.ExpressCode == expressCode);
        }

        /// <inheritdoc/>
        public async Task AddRangeStationInfoAsync(IReadOnlyCollection<StationInfo> stationInfo)
        {
            await _context.StationsInfo.AddRangeAsync(stationInfo);

            await _context.SaveChangesAsync();
        }
    }
}