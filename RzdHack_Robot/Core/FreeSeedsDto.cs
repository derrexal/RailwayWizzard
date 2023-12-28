using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RzdHack_Robot.Core
{
    public class FreeSeedsDto
    {
        public string serviceClass { get; set; } //Класс обслуживания
        public string trainCarNumber { get; set; } // Номер вагона
        public IList<string> seedsNumber { get; set; } // Номера мест
    }
}
