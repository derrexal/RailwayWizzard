using RailwayWizzard.Core;

namespace RailwayWizzard.B2B
{
    public interface IPassRzd
    {
        public Task<List<RootStations>> GetStations(string inputStation);
        public Task<IList<string>> GetAvailableTimes(ScheduleDto scheduleDto);
        public Task<List<StationInfo>> StationValidate(string stationName);
    }
}