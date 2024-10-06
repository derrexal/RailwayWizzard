using RailwayWizzard.Core;

namespace RailwayWizzard.B2B
{
    public class ScheduleDto
    {
        public string StationFromName { get; set; }

        public StationInfo? StationFrom { get; set; }

        public string StationToName { get; set; }

        public StationInfo? StationTo { get; set; }

        public string Date { get; set; }
    }
}
