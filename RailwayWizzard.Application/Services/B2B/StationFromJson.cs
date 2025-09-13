namespace RailwayWizzard.Application.Services.B2B
{
    /// <summary>
    /// Класс станции получаемой путем десериализации из json.
    /// </summary>
    [Obsolete("Эта модель использовалась на прошлой версии API. (https://pass.rzd.ru/suggester/?stationNamePart={uriInputStation}&lang=ru;)")]
    public class StationFromJson
    {
        public string n { get; set; } = null!;
        public int c { get; set; }
        public int S { get; set; }
        public int L { get; set; }  
        public bool? ss { get; set; }
    }
}
