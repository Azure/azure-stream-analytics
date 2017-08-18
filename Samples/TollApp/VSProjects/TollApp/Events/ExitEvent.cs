using System;
using System.Globalization;
using Newtonsoft.Json;

namespace TollApp.Events
{
    public class ExitEvent : TollEvent
    {
        #region Properties

        public DateTime ExitTime { get; set; }

        #endregion

        #region Constructor

        public ExitEvent(int tollId, DateTime exitTime, string licence)
        {
            TollId = tollId;
            ExitTime = exitTime;
            LicensePlate = licence;
        }

        #endregion

        #region Public Methods

        public override string Format()
        {
            return JsonConvert.SerializeObject(new
            {
                TollId = TollId.ToString(CultureInfo.InvariantCulture),
                ExitTime = ExitTime.ToString("o"),
                LicensePlate,
            });
        }

        #endregion

    }
}
