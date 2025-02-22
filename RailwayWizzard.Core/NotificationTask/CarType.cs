using System.ComponentModel;

namespace RailwayWizzard.Core.NotificationTask;

/// <summary>
/// Класс обслуживания.
/// </summary>
public enum CarType
{
    [Description("Сидячий")]
    Sedentary = 1,

    [Description("Сидячий (бизнес)")]
    SedentaryBusiness = 2,

    [Description("Плацкарт верхнее")]
    ReservedSeatUpper = 3,

    [Description("Плацкарт нижнее")]
    ReservedSeatLower = 4,

    [Description("Плацкарт верхнее боковое")]
    ReservedSeatUpperSide = 5,

    [Description("Плацкарт нижнее боковое")]
    ReservedSeatLowerSide = 6,

    [Description("Купе верхнее")]
    CompartmentUpper = 7,

    [Description("Купе нижнее")]
    CompartmentLower = 8,

    [Description("СВ")]
    Luxury = 9
}