namespace RailwayWizzard.Rzd.DataEngine.Core;

public class TrainInfo
{
    public List<CarGroup> CarGroups { get; } = new();
    public DateTime? DepartureDateTime { get; set; }
    public DateTime? LocalDepartureDateTime { get; set; }
    public string DisplayTrainNumber { get; set; } = null!;
}