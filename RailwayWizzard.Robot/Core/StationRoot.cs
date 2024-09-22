namespace RailwayWizzard.Robot.Core
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Root
    {
        public List<City> city { get; set; }
        public List<Train> train { get; set; }
        public List<Avium> avia { get; set; }
    }

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
        public bool hasAeroExpress { get; set; }
    }

    public class City
    {
        public string nodeId { get; set; }
        public string expressCode { get; set; }
        public string name { get; set; }
        public string nodeType { get; set; }
        public string transportType { get; set; }
        public string region { get; set; }
        public string regionIso { get; set; }
        public string countryIso { get; set; }
        public string busCode { get; set; }
        public string suburbanCode { get; set; }
        public string foreignCode { get; set; }
        public string expressCodes { get; set; }
        public bool hasAeroExpress { get; set; }
        public string aviaCode { get; set; }
    }

    public class Train
    {
        public string nodeId { get; set; }
        public string expressCode { get; set; }
        public string name { get; set; }
        public string nodeType { get; set; }
        public string transportType { get; set; }
        public string region { get; set; }
        public string regionIso { get; set; }
        public string countryIso { get; set; }
        public string suburbanCode { get; set; }
        public string foreignCode { get; set; }
        public bool hasAeroExpress { get; set; }
    }
}