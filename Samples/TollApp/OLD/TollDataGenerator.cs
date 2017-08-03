//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TollApp
{
    /// <summary>
    /// This class generates two continus streams of events - Cars entering toll station and cars exiting toll station.
    /// </summary>
    public class TollDataGenerator
    {
        private const int MaxTollId = 100;

        // One of the toll stations will intentionally be missing exit events to demonstrate use of outer left join
        private const int TollIdWithFailedExitSensor = 5;

        private static readonly CarModel[] CarModels = 
        {
            new CarModel("Toyota", "Camry", 1, 0),
            new CarModel("Ford", "Taurus", 1, 0),
            new CarModel("Toyota", "Corolla", 1, 0),
            new CarModel("Honda", "CRV", 1, 0),
            new CarModel("Toyota", "4x4", 1, 0),
            new CarModel("Honda", "Accord", 1, 0),
            new CarModel("Ford", "Mustang", 1, 0),
            new CarModel("Volvo", "S80", 1, 0),
            new CarModel("Volvo", "C30", 1, 0),
            new CarModel("Volvo", "V70", 1, 0),
            new CarModel("Toyota", "Rav4", 1, 0),
            new CarModel("Ford", "Focus", 1, 0),
            new CarModel("Chevy", "Malibu", 1, 0),
            new CarModel("Mac", "Granite", 2, 2.710),
            new CarModel("Kenworth", "T680", 2, 4.320),
            new CarModel("Peterbilt", "389", 2, 2.675),
            new CarModel("Honda", "Civic", 1, 0),
        };

        private static readonly string[] States = 
        {
            "WA",
            "OR",
            "CA",
            "AL",
            "TX",
            "NJ",
            "CT",
            "PA"
        };

        private readonly Random random;
        private readonly EventBuffer eventBuffer;
        private readonly Registration[] commercialVehicleRegistration;

        private TollDataGenerator(Random r)
        {
            random = r;
            eventBuffer = new EventBuffer();
            commercialVehicleRegistration = JsonConvert.DeserializeObject<Registration[]>(File.ReadAllText(@"Data\Registration.json"));
        }

        public static TollDataGenerator Generator()
        {
            return new TollDataGenerator(new Random());
        }

        public void Next(DateTime startTime, TimeSpan interval, int n)
        {
            for (int i = 0; i < n; i++)
            {
                var carModel = CarModels[random.Next(CarModels.Length)];
                var entryTime = startTime + TimeSpan.FromMilliseconds(random.Next((int)interval.TotalMilliseconds));
                var exitTime = entryTime + TimeSpan.FromSeconds(random.Next(60, 160));
                var tollId = random.Next(MaxTollId);
                var state = States[random.Next(States.Length)];
                var tollAmount = GetTollAmount(carModel);
                var tag = random.Next(100000000, 999999999);

                string licence;

                // For commercial vehicle pick license number from the reference data. Use random value for others.
                if (carModel.VehicleType == 2)
                {
                    licence =
                        commercialVehicleRegistration[random.Next(commercialVehicleRegistration.Length)].LicensePlate;
                }
                else
                {
                    licence = GetLicenceNumber();
                }
                eventBuffer.Add(entryTime, new EntryEvent(tollId, entryTime, licence, state, carModel, tollAmount, tag));

                if (tollId != TollIdWithFailedExitSensor)
                {
                    eventBuffer.Add(exitTime, new ExitEvent(tollId, exitTime, licence));
                }
            }
        }

        public IEnumerable<TollEvent> GetEvents(DateTime startTime)
        {
            return eventBuffer.GetEvents(startTime);
        }

        private double GetTollAmount(CarModel model)
        {
            if (model.VehicleType == 1)
            {
                return 4 + random.Next(3);
            }
            else
            {
                return 15 + random.Next(20);
            }
        }

        private string GetLicenceNumber()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor((26 * random.NextDouble()) + 65)));
                builder.Append(ch);
            }

            builder.AppendFormat(" {0}", random.Next(1000, 10000));
            return builder.ToString();
        }

        private void GenerateReferenceData(string fileName)
        {
            using (var file = File.CreateText(fileName))
            {
                file.WriteLine(@"LicensePlate,RegistrationId,Status");
                for (int i = 0; i < 10000; i++)
                {
                    file.WriteLine(
                        string.Join(
                        ",", 
                        GetLicenceNumber(),
                        random.Next(100000000, 999999999).ToString(CultureInfo.InvariantCulture),
                        random.Next(0, 2).ToString(CultureInfo.InvariantCulture)));
                }
            }
        }
    }
}
