using Newtonsoft.Json;

namespace RzdHack_Robot.Core
{
    /* 
     * Кажется этот класс сделан чтобы из бота не слать кучу лишних данных
     * Т.к. сейчас мы вообще не будем слать данные - класс не нужен?
     */
    public class RailwayInput
    {
        [JsonProperty("DepartureStation")]
        public string DepartureStation { get; set; }

        [JsonProperty("ArrivalStation")]
        public string ArrivalStation { get; set; }

        [JsonProperty("DateFrom")]
        public string DateFrom { get; set; }

        [JsonProperty("CurrentTime")]
        public string CurrentTime { get; set; }

        [JsonProperty("UserID")]
        public long UserID { get; set; }
    }
}