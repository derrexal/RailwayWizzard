using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;


namespace RailwayWizzard.Core
{
    [Table("AppStationInfo")]
    public class StationInfo : Entity
    {
        public long ExpressCode { get; set; }
        public string StationName { get; set; }
    }
}
