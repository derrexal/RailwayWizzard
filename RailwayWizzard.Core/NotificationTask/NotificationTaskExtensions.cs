namespace RailwayWizzard.Core.NotificationTask;

public static class NotificationTaskExtensions
{
    public static string ToBotString(this NotificationTask task, string departureStation, string arrivalStation) =>
        $"<strong>{task.TrainNumber}</strong> {departureStation} - {arrivalStation} {task.DepartureDateTime:yyyy-MM-dd} {task.DepartureDateTime:t}";

}