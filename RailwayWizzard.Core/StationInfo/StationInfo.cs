using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.StationInfo;

[Table("AppStationInfo")]
public class StationInfo : Entity
{
    /// <summary>
    /// Код станции.
    /// </summary>
    public long ExpressCode { get; init; }
    
    /// <summary>
    /// Наименование станции.
    /// </summary>
    public string Name { get; init; } = null!;
}