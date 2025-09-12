using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.StationInfo;

[Table("AppStationInfoExtended")]
public class StationInfoExtended : Entity
{
    /// <summary>
    /// Код станции.
    /// </summary>
    [Required]
    public long ExpressCode { get; init; }
    
    /// <summary>
    /// Наименование станции.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;
    
    [Required]
    public string NodeId { get; init; } = null!;
    
    [Required]
    public string NodeType { get; init; } = null!;
}