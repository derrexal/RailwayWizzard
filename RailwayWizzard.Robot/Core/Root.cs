namespace RailwayWizzard.Rzd.DataEngine.Core;

public class Root
{
    public int? Id { get; set; }
    public List<TrainInfo> Trains { get; } = new();
}
