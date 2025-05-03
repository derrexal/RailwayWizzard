namespace RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;

/// <summary>
/// Модель запроса информации о рейсе.
/// </summary>
public record GetTrainInformationRequest(
    long DepartureStationCode, 
    long ArrivalStationCode, 
    DateTime DepartureDateTime, 
    // string FirewallToken = "", 
    bool GetTrainsFromSchedule=true);