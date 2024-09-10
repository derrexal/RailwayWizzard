﻿namespace RailwayWizzard.B2B
{
    /// <summary>
    /// Класс станции получаемой путем десериализации из json
    /// </summary>
    public class StationFromJson
    {
        public string n { get; set; }
        public int c { get; set; }
        public int S { get; set; }
        public int L { get; set; }
        public bool? ss { get; set; }
    }
}