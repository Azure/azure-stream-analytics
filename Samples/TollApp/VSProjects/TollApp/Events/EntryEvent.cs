using System;
using System.Globalization;
using Newtonsoft.Json;
using TollApp.Models;

namespace TollApp.Events
{
    public class EntryEvent : TollEvent
    {
        #region Properties

        public DateTime EntryTime { get; set; }

        public CarModel CarModel { get; set; }

        public string State { get; set; }

        public double TollAmount { get; set; }

        public long Tag { get; set; }

        #endregion

        #region Constructor

        public EntryEvent(int tollId, DateTime entryTime, string licence, string state, CarModel carModel, double tollAmount, int tag)
        {
            TollId = tollId;
            EntryTime = entryTime;
            LicensePlate = licence;
            State = state;
            CarModel = carModel;
            TollAmount = tollAmount;
            Tag = tag;
        }

        #endregion

        #region Public Methods

        public override string Format()
        {
            return JsonConvert.SerializeObject(new
            {
                TollId = TollId.ToString(CultureInfo.InvariantCulture),
                EntryTime = EntryTime.ToString("o"),
                LicensePlate,
                State,
                CarModel.Make,
                CarModel.Model,
                VehicleType = CarModel.VehicleType.ToString(CultureInfo.InvariantCulture),
                VehicleWeight = CarModel.VehicleWeight.ToString(CultureInfo.InvariantCulture),
                Toll = TollAmount.ToString(CultureInfo.InvariantCulture),
                Tag = Tag.ToString(CultureInfo.InvariantCulture)
            });
        }

        #endregion
    }
}

