using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TollApp.Models;

namespace TollApp.Events
{

    /// <summary>
    /// The object containing the vehicle's details entering the tollbooth
    /// </summary>
    /// <see also cref="TollApp.Events.TollEvent" />
    public class EntryEvent : TollEvent
    {
        #region Properties
        [JsonProperty]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime EntryTime { get; set; }

        public CarModel CarModel { get; set; }

        public string State { get; set; }

        public double TollAmount { get; set; }

        public long Tag { get; set; }

        #endregion

    }
}


