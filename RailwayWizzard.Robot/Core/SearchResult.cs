
namespace RailwayWizzard.Robot.Core
{
    public class SearchResult
    {
        public string? CarType { get; set; }
        public int TotalPlace { get; set; }
        public double? Price { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SearchResult result &&
                   CarType == result.CarType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CarType);
        }
    }
}