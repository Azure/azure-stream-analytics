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
    public class Accelerometer : SensorBase
    {
        public Accelerometer()
            : base(SensorName.Accelerometer, SensorTagUuid.UUID_ACC_SERV, SensorTagUuid.UUID_ACC_CONF, SensorTagUuid.UUID_ACC_DATA)
        {
            
        }

        /// <summary>
        /// Extracts the values of the 3 axis from the raw data of the sensor.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <returns>Array of doubles with the size of 3</returns>
        public static double[] CalculateCoordinates(byte[] sensorData)
        {
            return CalculateCoordinates(sensorData, 1.0);
        }

        /// <summary>
        /// Extracts the values of the 3 axis from the raw data of the sensor,
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale">Allows you to scale the accelerometer values</param>
        /// <returns>Array of doubles with the size of 3</returns>
        public static double[] CalculateCoordinates(byte[] sensorData, double scale)
        {
            Validator.RequiresNotNull(sensorData, "sensorData");
            Validator.RequiresArgument(!double.IsNaN(scale), "scale cannot be double.NaN");
            if (scale == 0)
                throw new ArgumentOutOfRangeException("scale", "scale cannot be 0");
            return new double[] { sensorData[0] * scale, sensorData[1] * scale, sensorData[2] * scale };
        }

        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms.
        /// </summary>
        /// <param name="time">Period in 10 ms.</param>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public async Task SetReadPeriod(byte time)
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            if (time < 10)
                throw new ArgumentOutOfRangeException("time", "Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = deviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_ACC_PERI))[0];

            byte[] data = new byte[] { time };
            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());
            if(status == GattCommunicationStatus.Unreachable)
            {
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
            }
        }
    }
}
