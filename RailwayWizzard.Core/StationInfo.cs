using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;


namespace RailwayWizzard.Core
{
    [Table("AppStationInfo")]
    public class StationInfo: Entity
    {   
        public long ExpressCode { get; set; }
        public string StationName { get; set; }
    }
}
