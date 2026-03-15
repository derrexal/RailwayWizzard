using System;
using System.Collections.Generic;

namespace RailwayWizzard.Core.TrainBotTasks;

public enum TrainBotModeType
{
    SpecificTrain,
    TimeInterval
}

public enum TrainBotSeatMode
{
    AnySeat,
    ByServiceClass,
    SpecificCar
}

public abstract record TrainBotModeParams;

public sealed record SpecificTrainModeParams(TimeOnly Time) : TrainBotModeParams;

public sealed record TimeIntervalModeParams(
    TimeOnly? DepFrom,
    TimeOnly? DepTo,
    TimeOnly? ArrFrom,
    TimeOnly? ArrTo) : TrainBotModeParams;

public abstract record TrainBotSeatParams;

public sealed record AnySeatParams : TrainBotSeatParams;

public sealed record ByServiceClassParams(IReadOnlyList<string> Classes) : TrainBotSeatParams;

public sealed record SpecificCarParams(int Wagon) : TrainBotSeatParams;

public sealed record TrainBotTask(
    int TaskId,
    int UserId,
    long? TelegramId,
    string From,
    string To,
    DateOnly Date,
    short Seats,
    TrainBotModeType Mode,
    TrainBotSeatMode SeatMode,
    bool AccessibleOnly,
    TrainBotModeParams ModeParams,
    TrainBotSeatParams SeatParams
);
