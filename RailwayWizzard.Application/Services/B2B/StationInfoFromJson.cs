namespace RailwayWizzard.Application.Services.B2B;

public class StationInfoFromJson
{
    public List<City> city { get; } = new List<City>();
    // public List<Train> train { get; } = new List<Train>();
    // public List<Avium> avia { get; } = new List<Avium>();
}

public class City
{
    public string nodeId { get; set; }
    public string name { get; set; }
    public string nodeType { get; set; }
    public long expressCode { get; set; }
    public string transportType { get; set; }
    public string region { get; set; }
    public string regionIso { get; set; }
    public string countryIso { get; set; }
    public string aviaCode { get; set; }
    public string busCode { get; set; }
    public string suburbanCode { get; set; }
    public string foreignCode { get; set; }
    public string expressCodes { get; set; }
    public bool? hasAeroExpress { get; set; }
}

// ЖД станции не интересуют
public class Train
{
    public string nodeId { get; set; }
    public string name { get; set; }
    public string nodeType { get; set; }
    public long expressCode { get; set; }
    public string transportType { get; set; }
    public string region { get; set; }
    public string regionIso { get; set; }
    public string countryIso { get; set; }
    public string suburbanCode { get; set; }
    public string foreignCode { get; set; }
    public bool? hasAeroExpress { get; set; }
}

// Аэропорты не интересуют
public class Avium
{
    public string nodeId { get; set; }
    public string name { get; set; }
    public string nodeType { get; set; }
    public string transportType { get; set; }
    public string region { get; set; }
    public string regionIso { get; set; }
    public string countryIso { get; set; }
    public string aviaCode { get; set; }
    public bool? hasAeroExpress { get; set; }
}
