
namespace RailwayWizzard.Rzd.DataEngine.Core
{
    public class SearchResult
    {
        public string? CarType { get; set; }
        public int TotalPlace { get; set; }
        public double? Price { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SearchResult result &&
                   CarType == result.CarType &&
                   Price == result.Price;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CarType + Price);
        }
    }
}