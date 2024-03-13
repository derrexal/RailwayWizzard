namespace RailwayWizzard.Robot.Core
{
    public class Car
    {
        public int? carDataType { get; set; }
        public int? itype { get; set; }
        public string type { get; set; }
        public string typeLoc { get; set; }
        public int? freeSeats { get; set; }
        public int? pt { get; set; }
        public int? tariff { get; set; }
        public string servCls { get; set; }
        public bool? disabledPerson { get; set; }
    }

    public class CarType
    {
        public string type { get; set; }
        public int? itype { get; set; }
    }

    public class Root
    {
        public string number { get; set; }
        public string number2 { get; set; }
        public int? type { get; set; }
        public int? typeEx { get; set; }
        public int? depth { get; set; }
        public bool? @new { get; set; }
        public bool? elReg { get; set; }
        public bool? deferredPayment { get; set; }
        public bool? varPrice { get; set; }
        public int? code0 { get; set; }
        public int? code1 { get; set; }
        public bool? bEntire { get; set; }
        public string trainName { get; set; }
        public string brand { get; set; }
        public string carrier { get; set; }
        public string route0 { get; set; }
        public string route1 { get; set; }
        public int? routeCode0 { get; set; }
        public int? routeCode1 { get; set; }
        public string trDate0 { get; set; }
        public string trTime0 { get; set; }
        public string station0 { get; set; }
        public string station1 { get; set; }
        public string date0 { get; set; }
        public string time0 { get; set; }
        public string date1 { get; set; }
        public string time1 { get; set; }
        public string timeInWay { get; set; }
        public int? flMsk { get; set; }
        public int? train_id { get; set; }
        public List<Car>? cars { get; set; }
        public bool? disabledType { get; set; }
        public int? addCompLuggageNum { get; set; }
        public bool? addCompLuggage { get; set; }
        public bool? addHandLuggage { get; set; }
        public bool? nonRefundable { get; set; }
        public bool? brandLogo { get; set; }
        public int? brandId { get; set; }
        public string test { get; set; }
        public int? testSize { get; set; }
        public List<CarType>? carTypes { get; set; }
        public bool? addGoods { get; set; }
        public List<object>? hints { get; set; }
        public bool? bFirm { get; set; }
        public bool? addFood { get; set; }
    }
}
