namespace RailwayWizzard.Rzd.DataEngine.Core
{
    public class StationRoot
    {
        public List<City> city { get; set; } = null!;
        public List<Train> train { get; set; } = null!;
        public List<Avium> avia { get; set; } = null!;
    }

    public class Avium
    {
        public string nodeId { get; set; } = null!;
        public string name { get; set; } = null!;
        public string nodeType { get; set; } = null!;
        public string transportType { get; set; } = null!;
        public string region { get; set; } = null!;
        public string regionIso { get; set; } = null!;
        public string countryIso { get; set; } = null!;
        public string aviaCode { get; set; } = null!;
        public bool hasAeroExpress { get; set; }
    }

    public class City
    {
        public string nodeId { get; set; } = null!;
        public string expressCode { get; set; } = null!;
        public string name { get; set; } = null!;
        public string nodeType { get; set; } = null!;
        public string transportType { get; set; } = null!;
        public string region { get; set; } = null!;
        public string regionIso { get; set; } = null!;
        public string countryIso { get; set; } = null!;
        public string busCode { get; set; } = null!;
        public string suburbanCode { get; set; } = null!;
        public string foreignCode { get; set; } = null!;
        public string expressCodes { get; set; } = null!;
        public bool hasAeroExpress { get; set; }
        public string aviaCode { get; set; } = null!;
    }

    public class Train
    {
        public string nodeId { get; set; } = null!;
        public string expressCode { get; set; } = null!;
        public string name { get; set; } = null!;
        public string nodeType { get; set; } = null!;
        public string transportType { get; set; } = null!;
        public string region { get; set; } = null!;
        public string regionIso { get; set; } = null!;
        public string countryIso { get; set; } = null!;
        public string suburbanCode { get; set; } = null!;
        public string foreignCode { get; set; } = null!;
        public bool hasAeroExpress { get; set; }
    }
}