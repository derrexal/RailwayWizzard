namespace RailwayWizzard.Robot.Core
{
        public class AvailableBaggageType
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public object Description { get; set; }
        public object CarBaggageInfo { get; set; }
    }

    public class CarGroup
    {
        public List<string> Carriers { get; } = new List<string>();
        public List<string> CarrierDisplayNames { get; } = new List<string>();
        public List<string> ServiceClasses { get; } = new List<string>();
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public string CarType { get; set; }
        public string CarTypeName { get; set; }
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
        public string AvailabilityIndication { get; set; }
        public List<string> CarDescriptions { get; } = new List<string>();
        public string ServiceClassNameRu { get; set; }
        public string ServiceClassNameEn { get; set; }
        public List<string> InternationalServiceClasses { get; } = new List<string>();
        public List<double?> ServiceCosts { get; } = new List<double?>();
        public bool? IsBeddingSelectionPossible { get; set; }
        public List<string> BoardingSystemTypes { get; } = new List<string>();
        public bool? HasElectronicRegistration { get; set; }
        public bool? HasGenderCabins { get; set; }
        public bool? HasPlaceNumeration { get; set; }
        public bool? HasPlacesNearPlayground { get; set; }
        public bool? HasPlacesNearPets { get; set; }
        public bool HasPlacesForDisabledPersons { get; set; } //был nullable
        public bool? HasPlacesNearBabies { get; set; }
        public List<AvailableBaggageType> AvailableBaggageTypes { get; } = new List<AvailableBaggageType>();
        public bool? HasNonRefundableTariff { get; set; }
        public List<Discount> Discounts { get; } = new List<Discount>();
        public List<object> AllowedTariffs { get; } = new List<object>();
        public string InfoRequestSchema { get; set; }
        public int TotalPlaceQuantity { get; set; } //был nullable
        public List<string> PlaceReservationTypes { get; } = new List<string>();
        public bool? IsThreeHoursReservationAvailable { get; set; }
        public bool? IsMealOptionPossible { get; set; }
        public bool? IsAdditionalMealOptionPossible { get; set; }
        public bool? IsOnRequestMealOptionPossible { get; set; }
        public bool? IsTransitDocumentRequired { get; set; }
        public bool? IsInterstate { get; set; }
        public object ClientFeeCalculation { get; set; }
        public object AgentFeeCalculation { get; set; }
        public bool? HasNonBrandedCars { get; set; }
        public int? TripPointQuantity { get; set; }
        public bool? HasPlacesForBusinessTravelBooking { get; set; }
        public bool? IsCarTransportationCoaches { get; set; }
        public string ServiceClassName { get; set; }
        public bool? HasFssBenefit { get; set; }
        public string FakeCarType { get; set; }
    }

    public class DestinationStationInfo
    {
        public string StationName { get; set; }
        public string StationCode { get; set; }
        public string CnsiCode { get; set; }
        public string RegionName { get; set; }
        public string IsoCode { get; set; }
    }

    public class Discount
    {
        public string DiscountType { get; set; }
        public string Description { get; set; }
    }

    public class FinalStationInfo
    {
        public string StationName { get; set; }
        public string StationCode { get; set; }
        public string CnsiCode { get; set; }
        public string RegionName { get; set; }
        public string IsoCode { get; set; }
    }

    public class InitialStationInfo
    {
        public string StationName { get; set; }
        public string StationCode { get; set; }
        public string CnsiCode { get; set; }
        public string RegionName { get; set; }
        public string IsoCode { get; set; }
    }

    public class OriginStationInfo
    {
        public string StationName { get; set; }
        public string StationCode { get; set; }
        public string CnsiCode { get; set; }
        public string RegionName { get; set; }
        public string IsoCode { get; set; }
    }

    public class RootBigBrother
    {
        public string OriginStationCode { get; set; }
        public string DestinationStationCode { get; set; }
        public List<Train> Trains { get; } = new List<Train>();
        public object ClientFeeCalculation { get; set; }
        public object AgentFeeCalculation { get; set; }
        public string OriginCode { get; set; }
        public OriginStationInfo OriginStationInfo { get; set; }
        public int? OriginTimeZoneDifference { get; set; }
        public string DestinationCode { get; set; }
        public DestinationStationInfo DestinationStationInfo { get; set; }
        public int? DestinationTimeZoneDifference { get; set; }
        public string RoutePolicy { get; set; }
        public string DepartureTimeDescription { get; set; }
        public string ArrivalTimeDescription { get; set; }
        public bool? IsFromUkrain { get; set; }
        public bool? NotAllTrainsReturned { get; set; }
        public string BookingSystem { get; set; }
        public int? Id { get; set; }
        public string DestinationStationName { get; set; }
        public string OriginStationName { get; set; }
        public DateTime? MoscowDateTime { get; set; }
    }

    public class Train
    {
        public List<CarGroup> CarGroups { get; } = new List<CarGroup>();
        public bool? IsFromSchedule { get; set; }
        public bool? IsTourPackagePossible { get; set; }
        public int? CarTransportationsFreePlacesCount { get; set; }
        public object ActualMovement { get; set; }
        public object CategoryId { get; set; }
        public object ScheduleId { get; set; }
        public List<string> BaggageCarsThreads { get; } = new List<string>();
        public List<string> CarTransportationCoachesThreads { get; } = new List<string>();
        public string Provider { get; set; }
        public bool? IsWaitListAvailable { get; set; }
        public bool? HasElectronicRegistration { get; set; }
        public bool? HasCarTransportationCoaches { get; set; }
        public bool? HasDynamicPricingCars { get; set; }
        public bool? HasTwoStoreyCars { get; set; }
        public bool? HasSpecialSaleMode { get; set; }
        public List<string> Carriers { get; } = new List<string>();
        public List<string> CarrierDisplayNames { get; } = new List<string>();
        public int? Id { get; set; }
        public bool? IsBranded { get; set; }
        public string TrainNumber { get; set; }
        public string TrainNumberToGetRoute { get; set; }
        public string DisplayTrainNumber { get; set; }
        public string TrainDescription { get; set; }
        public string TrainName { get; set; }
        public string TrainNameEn { get; set; }
        public string TransportType { get; set; }
        public string OriginName { get; set; }
        public string InitialStationName { get; set; }
        public string OriginStationCode { get; set; }
        public OriginStationInfo OriginStationInfo { get; set; }
        public InitialStationInfo InitialStationInfo { get; set; }
        public string InitialTrainStationCode { get; set; }
        public string InitialTrainStationCnsiCode { get; set; }
        public string DestinationName { get; set; }
        public string FinalStationName { get; set; }
        public string DestinationStationCode { get; set; }
        public DestinationStationInfo DestinationStationInfo { get; set; }
        public FinalStationInfo FinalStationInfo { get; set; }
        public string FinalTrainStationCode { get; set; }
        public string FinalTrainStationCnsiCode { get; set; }
        public List<string> DestinationNames { get; } = new List<string>();
        public List<string> FinalStationNames { get; } = new List<string>();
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? LocalDepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public DateTime? LocalArrivalDateTime { get; set; }
        public List<DateTime?> ArrivalDateTimes { get; } = new List<DateTime?>();
        public List<DateTime?> LocalArrivalDateTimes { get; } = new List<DateTime?>();
        public DateTime? DepartureDateFromFormingStation { get; set; }
        public int? DepartureStopTime { get; set; }
        public int? ArrivalStopTime { get; set; }
        public double? TripDuration { get; set; }
        public int? TripDistance { get; set; }
        public bool? IsSuburban { get; set; }
        public bool? IsComponent { get; set; }
        public List<string> CarServices { get; } = new List<string>();
        public bool? IsSaleForbidden { get; set; }
        public bool? IsTicketPrintRequiredForBoarding { get; set; }
        public string BookingSystem { get; set; }
        public bool? IsVrStorageSystem { get; set; }
        public string PlacesStorageType { get; set; }
        public List<string> BoardingSystemTypes { get; } = new List<string>();
        public object TrainBrandCode { get; set; }
        public List<string> TrainClassNames { get; } = new List<string>();
        public string ServiceProvider { get; set; }
        public string DestinationStationName { get; set; }
        public string OriginStationName { get; set; }
        public bool? IsPlaceRangeAllowed { get; set; }
        public bool? IsTrainRouteAllowed { get; set; }
    }


}
