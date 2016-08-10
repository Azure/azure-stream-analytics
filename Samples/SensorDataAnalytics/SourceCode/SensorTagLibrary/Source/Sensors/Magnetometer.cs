#region License
// Copyright © X2Codinglab Sebastian Lang 2013
// <author>Sebastian Lang</author>
// <project>TI Sensor Tag Library</project>
// <website>https://sensortag.codeplex.com/</website>
// <license>See https://sensortag.codeplex.com/license </license>
#endregion License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using X2CodingLab.SensorTag.Exceptions;
using System.Runtime.InteropServices.WindowsRuntime;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public class Magnetometer : SensorBase
    {
        public Magnetometer()
            : base(SensorName.Magnetometer, SensorTagUuid.UUID_MAG_SERV, SensorTagUuid.UUID_MAG_CONF, SensorTagUuid.UUID_MAG_DATA)
        {
            
        }

        /// <summary>
        /// Extracts the three axis from the raw sensor data and scales it.
        /// http://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf?fpsp=1
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <returns></returns>
        public static float[] CalculateCoordinates(byte[] sensorData)
        {
            Validator.RequiresNotNull(sensorData);
            return new float[] { BitConverter.ToInt16(sensorData, 0) * (2000f / 65536f), 
                BitConverter.ToInt16(sensorData, 2) * (2000f / 65536f), 
                BitConverter.ToInt16(sensorData, 4) * (2000f / 65536f)};
        }

        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms.
        /// </summary>
        /// <param name="time">Period in 10 ms</param>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public async Task SetReadPeriod(byte time)
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            if (time < 10)
                throw new ArgumentOutOfRangeException("time", "Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = deviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_MAG_PERI))[0];

            byte[] data = new byte[] { time };
            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
            }
        }
    }
}
