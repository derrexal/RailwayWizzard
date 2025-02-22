//TODO: разбить по файлам
namespace RailwayWizzard.Rzd.DataEngine.Core
{
    public class AvailableBaggageType
    {
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
        public object Description { get; set; } = null!;
        public object CarBaggageInfo { get; set; } = null!;
    }

    public class CarGroup
    {
        public List<string> Carriers { get; } = new();
        public List<string> CarrierDisplayNames { get; } = new();
        public List<string> ServiceClasses { get; } = new();
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public string CarType { get; set; } = null!;
        public string CarTypeName { get; set; } = null!;
        public int? PlaceQuantity { get; set; }
        public int? LowerPlaceQuantity { get; set; }
        public int? UpperPlaceQuantity { get; set; }
        public int? LowerSidePlaceQuantity { get; set; }
        public int? UpperSidePlaceQuantity { get; set; }
        public int? MalePlaceQuantity { get; set; }
        public int? FemalePlaceQuantity { get; set; }
        public int? EmptyCabinQuantity { get; set; }
        public int? MixedCabinQuantity { get; set; }
        public bool? IsSaleForbidden { get; set; }
        public string AvailabilityIndication { get; set; } = null!;
        public List<string> CarDescriptions { get; } = new();
        public string ServiceClassNameRu { get; set; } = null!;
        public string ServiceClassNameEn { get; set; } = null!;
        public List<string> InternationalServiceClasses { get; } = new();
        public List<double?> ServiceCosts { get; } = new();
        public bool? IsBeddingSelectionPossible { get; set; }
        public List<string> BoardingSystemTypes { get; } = new();
        public bool? HasElectronicRegistration { get; set; }
        public bool? HasGenderCabins { get; set; }
        public bool? HasPlaceNumeration { get; set; }
        public bool? HasPlacesNearPlayground { get; set; }
        public bool? HasPlacesNearPets { get; set; }
        public bool HasPlacesForDisabledPersons { get; set; }
        public bool? HasPlacesNearBabies { get; set; }
        public List<AvailableBaggageType> AvailableBaggageTypes { get; } = new();
        public bool? HasNonRefundableTariff { get; set; }
        public List<Discount> Discounts { get; } = new();
        public List<object> AllowedTariffs { get; } = new();
        public string InfoRequestSchema { get; set; } = null!;
        public int TotalPlaceQuantity { get; set; }
        public List<string> PlaceReservationTypes { get; } = new();
        public bool? IsThreeHoursReservationAvailable { get; set; }
        public bool? IsMealOptionPossible { get; set; }
        public bool? IsAdditionalMealOptionPossible { get; set; }
        public bool? IsOnRequestMealOptionPossible { get; set; }
        public bool? IsTransitDocumentRequired { get; set; }
        public bool? IsInterstate { get; set; }
        public object ClientFeeCalculation { get; set; } = null!;
        public object AgentFeeCalculation { get; set; } = null!;
        public bool? HasNonBrandedCars { get; set; }
        public int? TripPointQuantity { get; set; }
        public bool? HasPlacesForBusinessTravelBooking { get; set; }
        public bool? IsCarTransportationCoaches { get; set; }
        public string ServiceClassName { get; set; } = null!;
        public bool? HasFssBenefit { get; set; }
        public string FakeCarType { get; set; } = null!;
    }

    public class DestinationStationInfo
    {
        public string StationName { get; set; } = null!;
        public string StationCode { get; set; } = null!;
        public string CnsiCode { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string IsoCode { get; set; } = null!;
    }

    public class Discount
    {
        public string DiscountType { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class FinalStationInfo
    {
        public string StationName { get; set; } = null!;
        public string StationCode { get; set; } = null!;
        public string CnsiCode { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string IsoCode { get; set; } = null!;
    }

    public class InitialStationInfo
    {
        public string StationName { get; set; } = null!;
        public string StationCode { get; set; } = null!;
        public string CnsiCode { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string IsoCode { get; set; } = null!;
    }

    public class OriginStationInfo
    {
        public string StationName { get; set; } = null!;
        public string StationCode { get; set; } = null!;
        public string CnsiCode { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string IsoCode { get; set; } = null!;
    }

    public class RootShort
    {
        public int? Id { get; set; }
        public List<TrainShort> Trains { get; } = new();
    }

    public class TrainShort
    {
        public List<CarGroupShort> CarGroups { get; } = new();
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? LocalDepartureDateTime { get; set; }
        public string DisplayTrainNumber { get; set; } = null!;
    }

    public class CarGroupShort
    {
        public double? MinPrice { get; set; }
        
        public double? MaxPrice { get; set; }
        public string CarType { get; set; } = null!;
        public string CarTypeName { get; set; } = null!;
        public string? ServiceClassNameRu { get; set; }
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
    }

    [Obsolete]
    public class Root
    {
        public string OriginStationCode { get; set; } = null!;
        public string DestinationStationCode { get; set; } = null!;
        public List<TrainFull> Trains { get; } = new();
        public object ClientFeeCalculation { get; set; } = null!;
        public object AgentFeeCalculation { get; set; } = null!;
        public string OriginCode { get; set; } = null!;
        public OriginStationInfo OriginStationInfo { get; set; } = null!;
        public int? OriginTimeZoneDifference { get; set; }
        public string DestinationCode { get; set; } = null!;
        public DestinationStationInfo DestinationStationInfo { get; set; } = null!;
        public int? DestinationTimeZoneDifference { get; set; }
        public string RoutePolicy { get; set; } = null!;
        public string DepartureTimeDescription { get; set; } = null!;
        public string ArrivalTimeDescription { get; set; } = null!;
        public bool? IsFromUkrain { get; set; }
        public bool? NotAllTrainsReturned { get; set; }
        public string BookingSystem { get; set; } = null!;
        public int? Id { get; set; }
        public string DestinationStationName { get; set; } = null!;
        public string OriginStationName { get; set; } = null!;
        public DateTime? MoscowDateTime { get; set; }
    }

    public class RootDepartureTime
    {
        public List<TrainDepartureTime> Trains { get; } = new();
    }

    public class TrainDepartureTime
    {
        public DateTime DepartureDateTime { get; set; }
        public DateTime? LocalDepartureDateTime { get; set; }
    }

    public class TrainFull
    {
        public List<CarGroup> CarGroups { get; } = new();
        public bool? IsFromSchedule { get; set; }
        public bool? IsTourPackagePossible { get; set; }
        public int? CarTransportationsFreePlacesCount { get; set; }
        public object ActualMovement { get; set; } = null!;
        public object CategoryId { get; set; } = null!;
        public object ScheduleId { get; set; } = null!;
        public List<string> BaggageCarsThreads { get; } = new();
        public List<string> CarTransportationCoachesThreads { get; } = new();
        public string Provider { get; set; } = null!;
        public bool? IsWaitListAvailable { get; set; }
        public bool? HasElectronicRegistration { get; set; }
        public bool? HasCarTransportationCoaches { get; set; }
        public bool? HasDynamicPricingCars { get; set; }
        public bool? HasTwoStoreyCars { get; set; }
        public bool? HasSpecialSaleMode { get; set; }
        public List<string> Carriers { get; } = new();
        public List<string> CarrierDisplayNames { get; } = new();
        public int? Id { get; set; }
        public bool? IsBranded { get; set; }
        public string TrainNumber { get; set; } = null!;
        public string TrainNumberToGetRoute { get; set; } = null!;
        public string DisplayTrainNumber { get; set; } = null!;
        public string TrainDescription { get; set; } = null!;
        public string TrainName { get; set; } = null!;
        public string TrainNameEn { get; set; } = null!;
        public string TransportType { get; set; } = null!;
        public string OriginName { get; set; } = null!;
        public string InitialStationName { get; set; } = null!;
        public string OriginStationCode { get; set; } = null!;
        public OriginStationInfo OriginStationInfo { get; set; } = null!;
        public InitialStationInfo InitialStationInfo { get; set; } = null!;
        public string InitialTrainStationCode { get; set; } = null!;
        public string InitialTrainStationCnsiCode { get; set; } = null!;
        public string DestinationName { get; set; } = null!;
        public string FinalStationName { get; set; } = null!;
        public string DestinationStationCode { get; set; } = null!;
        public DestinationStationInfo DestinationStationInfo { get; set; } = null!;
        public FinalStationInfo FinalStationInfo { get; set; } = null!;
        public string FinalTrainStationCode { get; set; } = null!;
        public string FinalTrainStationCnsiCode { get; set; } = null!;
        public List<string> DestinationNames { get; } = new();
        public List<string> FinalStationNames { get; } = new();
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? LocalDepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public DateTime? LocalArrivalDateTime { get; set; }
        public List<DateTime?> ArrivalDateTimes { get; } = new();
        public List<DateTime?> LocalArrivalDateTimes { get; } = new();
        public DateTime? DepartureDateFromFormingStation { get; set; }
        public int? DepartureStopTime { get; set; }
        public int? ArrivalStopTime { get; set; }
        public double? TripDuration { get; set; }
        public int? TripDistance { get; set; }
        public bool? IsSuburban { get; set; }
        public bool? IsComponent { get; set; }
        public List<string> CarServices { get; } = new();
        public bool? IsSaleForbidden { get; set; }
        public bool? IsTicketPrintRequiredForBoarding { get; set; }
        public string BookingSystem { get; set; } = null!;
        public bool? IsVrStorageSystem { get; set; }
        public string PlacesStorageType { get; set; } = null!;
        public List<string> BoardingSystemTypes { get; } = new();
        public object TrainBrandCode { get; set; } = null!;
        public List<string> TrainClassNames { get; } = new();
        public string ServiceProvider { get; set; } = null!;
        public string DestinationStationName { get; set; } = null!;
        public string OriginStationName { get; set; } = null!;
        public bool? IsPlaceRangeAllowed { get; set; }
        public bool? IsTrainRouteAllowed { get; set; }
    }
}
