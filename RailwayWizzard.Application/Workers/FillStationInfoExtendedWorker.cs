using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RailwayWizzard.Application.Services.B2B;
using RailwayWizzard.Common;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Infrastructure;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

namespace RailwayWizzard.Application.Workers
{
    public class FillStationInfoExtendedWorker : BackgroundService
    {
        private const int RunInterval = 1000 * 5; // 5 second
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<FillStationInfoExtendedWorker> _logger;

        public FillStationInfoExtendedWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<FillStationInfoExtendedWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(FillStationInfoExtendedWorker)} running at: {DateTimeExtensions.MoscowNow} Moscow time");

                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RailwayWizzardAppContext>();
            var getStationsByNameService = scope.ServiceProvider.GetRequiredService<IGetStationsByNameService>();

            try
            {
                var newStationsCount = await context.StationsInfoExtended.CountAsync(cancellationToken);
                Console.WriteLine($"Добавлено новых станций: {newStationsCount}");

                var actualStationIdList = await context.NotificationTasks
                    .Select(nt => nt.DepartureStationId)
                    .Concat(context.NotificationTasks.Select(nt => nt.ArrivalStationId))
                    .Distinct()
                    .ToArrayAsync(cancellationToken);
                                
                Console.WriteLine($"Начало миграции");
                
                foreach (var actualStationId in actualStationIdList)
                {
                    // Проверяем, есть ли уже эта станция в новой таблице
                    var alreadyMigrated = await context.StationsInfoExtended
                        .AnyAsync(se => se.Id == actualStationId, cancellationToken);
                    if (alreadyMigrated) continue;
                    
                    Console.WriteLine($"Добавляем станцию с ID: {actualStationId}");

                    // Получаем сущность старой станции
                    var oldStation = context.StationsInfo.FirstOrDefault(s => s.Id == actualStationId);
                    if (oldStation == null)
                    {
                        _logger.LogError($"Error. Station {actualStationId} not found in table AppStationInfo");
                        continue;
                    }

                    // Пытаемся найти станцию в новом API
                    var newStationsData = await getStationsByNameService.GetDataExtendedAsync(oldStation.Name);
                    var extendedStation = JsonConvert.DeserializeObject<StationInfoFromJson>(newStationsData);
                    if (extendedStation == null)
                    {
                        _logger.LogError($"Error. Request new stations return {extendedStation} resulted is empty");
                        continue;
                    }

                    var result = extendedStation.city.ToArray();
                    
                    var newStationCompleteMatch = result.FirstOrDefault(station => station.expressCode == oldStation.ExpressCode && station.name == oldStation.Name);
                    if (newStationCompleteMatch is not null)
                    {
                        context.StationsInfoExtended.Add(new StationInfoExtended
                        {
                            Id = oldStation.Id,
                            ExpressCode = oldStation.ExpressCode,
                            Name = newStationCompleteMatch.name,
                            NodeId = newStationCompleteMatch.nodeId,
                            NodeType = newStationCompleteMatch.nodeType,
                        });
                    }
                    else
                    {
                        var newStationNoCompleteMatch = result.FirstOrDefault(station => station.expressCode == oldStation.ExpressCode);

                        if (newStationNoCompleteMatch is not null)
                        {
                            context.StationsInfoExtended.Add(new StationInfoExtended
                            {
                                Id = oldStation.Id,
                                ExpressCode = oldStation.ExpressCode,
                                Name = newStationNoCompleteMatch.name,
                                NodeId = newStationNoCompleteMatch.nodeId,
                                NodeType = newStationNoCompleteMatch.nodeType,
                            });
                        }
                        else
                        {
                            context.StationsInfoExtended.Add(new StationInfoExtended
                            {
                                Id = oldStation.Id,
                                ExpressCode = oldStation.ExpressCode,
                                Name = $"NOT_FOUND_LEGACY_{oldStation.Name}",
                                NodeId = "LEGACY",
                                NodeType = "LEGACY",
                            });
                        }
                    }

                    await context.SaveChangesAsync(cancellationToken);
                    await Task.Delay(RunInterval, cancellationToken);
                }
                
                Console.WriteLine($"Конец миграции");
            }
            
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(FillStationInfoExtendedWorker)} {ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(FillStationInfoExtendedWorker)} stopped at: {DateTimeExtensions.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}