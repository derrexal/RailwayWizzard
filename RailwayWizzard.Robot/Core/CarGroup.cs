namespace RailwayWizzard.Rzd.DataEngine.Core;

public class CarGroup
{
    public double? MinPrice { get; set; }
        
    public double? MaxPrice { get; set; }
    public string CarType { get; set; } = null!;
    public string CarTypeName { get; set; } = null!;

    public string FakeCarType { get; set; } = null!;
    public string ServiceClassName { get; set; } = null!;
    public string? ServiceClassNameRu { get; set; }
    public string ServiceClassNameEn { get; set; } = null!;

    public bool HasPlacesForDisabledPersons { get; set; }
        
    public bool HasPlacesForBusinessTravelBooking { get; set; }
        
    public int TotalPlaceQuantity { get; set; }
        
    public int? PlaceQuantity { get; set; }
        
    public int LowerPlaceQuantity { get; set; }
        
    public int LowerSidePlaceQuantity { get; set; }

    public int UpperPlaceQuantity { get; set; }
        
    public int UpperSidePlaceQuantity { get; set; }

    public int? MalePlaceQuantity { get; set; }
        
    public int? FemalePlaceQuantity { get; set; }
        
    public int? EmptyCabinQuantity { get; set; }
        
    public int? MixedCabinQuantity { get; set; }
        
    public bool IsBusinessClass =>
        HasPlacesForBusinessTravelBooking
        || string.Equals(ServiceClassNameEn, "Business", StringComparison.OrdinalIgnoreCase)
        || string.Equals(ServiceClassNameRu, "Бизнес класс", StringComparison.OrdinalIgnoreCase)
        || string.Equals(ServiceClassName, "Бизнес класс", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FakeCarType, "Business", StringComparison.OrdinalIgnoreCase);
}