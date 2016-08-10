#region License
// Copyright © X2Codinglab Sebastian Lang 2013
// <author>Sebastian Lang</author>
// <project>TI Sensor Tag Library</project>
// <website>https://sensortag.codeplex.com/</website>
// <license>See https://sensortag.codeplex.com/license </license>
#endregion License
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public class SimpleKeyService : SensorBase
    {
        public SimpleKeyService()
            : base(SensorName.SimpleKeyService, SensorTagUuid.UUID_KEY_SERV, null, SensorTagUuid.UUID_KEY_DATA)
        {
            
        }

        public static bool LeftKeyHit(byte[] sensorData)
        {
            Validator.RequiresNotNull(sensorData);
            Validator.RequiresArgument(sensorData.Length >= 1, "sensorData has to be at least one byte.");
            return (new BitArray(sensorData))[1];
        }

        public static bool RightKeyHit(byte[] sensorData)
        {
            Validator.RequiresNotNull(sensorData);
            Validator.RequiresArgument(sensorData.Length >= 1, "sensorData has to be at least one byte.");
            return (new BitArray(sensorData))[0];
        }

        public static bool SideKeyHit(byte[] sensorData)
        {
            Validator.RequiresNotNull(sensorData);
            Validator.RequiresArgument(sensorData.Length >= 1, "sensorData has to be at least one byte.");
            return (new BitArray(sensorData))[2];
        }

#pragma warning disable

        /// <summary>
        /// The simple key service doens't need to be enabled
        /// </summary>
        /// <returns></returns>
        public async override Task EnableSensor()
        {
            // not possible in this case
        }

        /// <summary>
        /// The simple key service doesn't need to be disabled
        /// </summary>
        /// <returns></returns>
        public async override Task DisableSensor()
        {
            // not possible in this case
        }

#pragma warning restore
    }
}
