using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RzdHack_Robot.Core
{
    public class RoutesParam
    {
        public int ArrivalStationCode { get; set; }
        public int DepartureStationCode { get; set; }
        public DateOnly DepartureDate { get; set; }
    }
}
