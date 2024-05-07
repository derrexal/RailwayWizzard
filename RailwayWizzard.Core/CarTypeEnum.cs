using System.ComponentModel;

namespace RailwayWizzard.Core;

/// <summary>
/// Типы вагона
/// </summary>
public enum CarTypeEnum
{
    [Description("Сидячий")]
    Sedentary = 1,

    [Description("Плацкарт")]
    ReservedSeat = 2,

    [Description("Купе")]
    Compartment = 3,

    [Description("СВ")]
    Luxury = 4
}