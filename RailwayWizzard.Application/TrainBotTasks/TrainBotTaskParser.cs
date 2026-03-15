using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RailwayWizzard.Core.TrainBotOutbox;
using RailwayWizzard.Core.TrainBotTasks;

namespace RailwayWizzard.Application.TrainBotTasks;

public static class TrainBotTaskParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static bool TryParseCreated(TrainBotMessage message, out TrainBotTask task, out string error) =>
        TryParseCreated(message.Payload, out task, out error);

    public static bool TryParseCreated(string payload, out TrainBotTask task, out string error)
    {
        task = default!;
        error = string.Empty;

        TrainBotOutboxPayload? outbox;
        try
        {
            outbox = JsonSerializer.Deserialize<TrainBotOutboxPayload>(payload, Options);
        }
        catch (JsonException ex)
        {
            error = $"Invalid outbox payload JSON: {ex.Message}";
            return false;
        }

        if (outbox is null)
        {
            error = "Outbox payload is empty.";
            return false;
        }

        if (!string.Equals(outbox.Type, "notification_task.created", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Unsupported outbox type '{outbox.Type}'.";
            return false;
        }

        if (outbox.Draft is null)
        {
            error = "Draft is missing.";
            return false;
        }

        var draft = outbox.Draft;

        if (string.IsNullOrWhiteSpace(draft.From) || string.IsNullOrWhiteSpace(draft.To))
        {
            error = "Stations are missing.";
            return false;
        }

        if (!TryParseDate(draft.Date, out var date, out error))
            return false;

        if (draft.Seats <= 0)
        {
            error = "Seats count is invalid.";
            return false;
        }

        var mode = ParseMode(draft.Mode);
        var seatMode = ParseSeatMode(draft.SeatMode);

        if (!TryBuildModeParams(mode, draft, out var modeParams, out error))
            return false;

        if (!TryBuildSeatParams(seatMode, draft, out var seatParams, out error))
            return false;

        task = new TrainBotTask(
            outbox.TaskId,
            outbox.UserId,
            outbox.TelegramId,
            draft.From,
            draft.To,
            date,
            (short)draft.Seats,
            mode,
            seatMode,
            draft.AccessibleOnly,
            modeParams,
            seatParams);

        return true;
    }

    private static TrainBotModeType ParseMode(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "time_interval" => TrainBotModeType.TimeInterval,
            _ => TrainBotModeType.SpecificTrain
        };

    private static TrainBotSeatMode ParseSeatMode(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "any_seat" => TrainBotSeatMode.AnySeat,
            "specific_car" => TrainBotSeatMode.SpecificCar,
            _ => TrainBotSeatMode.ByServiceClass
        };

    private static bool TryBuildModeParams(
        TrainBotModeType mode,
        TrainBotDraft draft,
        out TrainBotModeParams modeParams,
        out string error)
    {
        modeParams = default!;
        error = string.Empty;

        if (mode == TrainBotModeType.SpecificTrain)
        {
            if (!TryParseTime(draft.Time, out var time, out error))
                return false;

            modeParams = new SpecificTrainModeParams(time);
            return true;
        }

        if (!TryParseOptionalTime(draft.DepFrom, out var depFrom, out error))
        {
            error = $"Invalid dep_from: {error}";
            return false;
        }

        if (!TryParseOptionalTime(draft.DepTo, out var depTo, out error))
        {
            error = $"Invalid dep_to: {error}";
            return false;
        }

        if (!TryParseOptionalTime(draft.ArrFrom, out var arrFrom, out error))
        {
            error = $"Invalid arr_from: {error}";
            return false;
        }

        if (!TryParseOptionalTime(draft.ArrTo, out var arrTo, out error))
        {
            error = $"Invalid arr_to: {error}";
            return false;
        }

        modeParams = new TimeIntervalModeParams(depFrom, depTo, arrFrom, arrTo);
        return true;
    }

    private static bool TryBuildSeatParams(
        TrainBotSeatMode seatMode,
        TrainBotDraft draft,
        out TrainBotSeatParams seatParams,
        out string error)
    {
        seatParams = default!;
        error = string.Empty;

        switch (seatMode)
        {
            case TrainBotSeatMode.AnySeat:
                seatParams = new AnySeatParams();
                return true;
            case TrainBotSeatMode.ByServiceClass:
                seatParams = new ByServiceClassParams(draft.Classes ?? new List<string>{});
                return true;
            case TrainBotSeatMode.SpecificCar:
                if (!draft.Wagon.HasValue || draft.Wagon.Value <= 0)
                {
                    error = "Wagon is missing.";
                    return false;
                }

                seatParams = new SpecificCarParams(draft.Wagon.Value);
                return true;
            default:
                error = $"Unsupported seat mode: {seatMode}.";
                return false;
        }
    }

    private static bool TryParseDate(string? value, out DateOnly date, out string error)
    {
        date = default;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Departure date is missing.";
            return false;
        }

        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            error = "Departure date has invalid format.";
            return false;
        }

        return true;
    }

    private static bool TryParseTime(string? value, out TimeOnly time, out string error)
    {
        time = default;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Departure time is missing.";
            return false;
        }

        if (TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            return true;

        if (TimeOnly.TryParseExact(value, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            return true;

        error = "Departure time has invalid format.";
        return false;
    }

    private static bool TryParseOptionalTime(string? value, out TimeOnly? time, out string error)
    {
        time = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!TryParseTime(value, out var parsed, out error))
            return false;

        time = parsed;
        return true;
    }

    private sealed class TrainBotOutboxPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("task_id")]
        public int TaskId { get; init; }

        [JsonPropertyName("user_id")]
        public int UserId { get; init; }

        [JsonPropertyName("telegram_id")]
        public long? TelegramId { get; init; }

        [JsonPropertyName("draft")]
        public TrainBotDraft? Draft { get; init; }
    }

    private sealed class TrainBotDraft
    {
        [JsonPropertyName("from")]
        public string From { get; init; } = string.Empty;

        [JsonPropertyName("to")]
        public string To { get; init; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; init; } = string.Empty;

        [JsonPropertyName("mode")]
        public string? Mode { get; init; }

        [JsonPropertyName("time")]
        public string? Time { get; init; }

        [JsonPropertyName("dep_from")]
        public string? DepFrom { get; init; }

        [JsonPropertyName("dep_to")]
        public string? DepTo { get; init; }

        [JsonPropertyName("arr_from")]
        public string? ArrFrom { get; init; }

        [JsonPropertyName("arr_to")]
        public string? ArrTo { get; init; }

        [JsonPropertyName("seats")]
        public int Seats { get; init; }

        [JsonPropertyName("seat_mode")]
        public string? SeatMode { get; init; }

        [JsonPropertyName("classes")]
        public List<string>? Classes { get; init; }

        [JsonPropertyName("wagon")]
        public int? Wagon { get; init; }

        [JsonPropertyName("accessible_only")]
        public bool AccessibleOnly { get; init; }
    }
}
