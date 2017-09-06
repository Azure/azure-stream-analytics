using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TollApp.Events
{
    /// <summary>
    /// The object containing the vehicle's details leaving the tollbooth
    /// </summary>
    /// <seealso cref="TollApp.Events.TollEvent" />
    public class ExitEvent : TollEvent
    {
        #region Properties
        [JsonProperty]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExitTime { get; set; }

        #endregion
    }
}
